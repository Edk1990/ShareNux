#region License Information (GPL v3)

/*
    ShareX - A program that allows you to take screenshots and share any file type
    Copyright (c) 2007-2026 ShareX Team

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)

#if !WINDOWS

using Avalonia.Threading;
using ShareX.HelpersLib;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace ShareX.HelpersLib.Input.Linux;

public sealed class X11HotkeyHost : IHotkeyHost
{
    private IntPtr _display;
    private IntPtr _rootWindow;
    private Thread _eventThread;
    private CancellationTokenSource _cts;
    private readonly object _x11Lock = new();
    private readonly object _idLock = new();
    private ushort _nextId;
    private readonly Stopwatch _repeatLimitTimer = Stopwatch.StartNew();
    private readonly ConcurrentDictionary<(int KeyCode, uint Modifiers), HotkeyInfo> _registeredHotkeys = new();

    public event HotkeyEventHandler HotkeyPress;
    public event EventHandler Closed;
#pragma warning disable CS0067
    public event EventHandler<NativeWindowMessageEventArgs> NativeMessageReceived;
#pragma warning restore CS0067

    public IntPtr Handle => _rootWindow;
    public bool IsDisposed { get; private set; }
    public int HotkeyRepeatLimit { get; set; } = 1000;

    public void Initialize()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        lock (_x11Lock)
        {
            if (_display != IntPtr.Zero) return;

            _display = XOpenDisplay(null);
            if (_display == IntPtr.Zero)
            {
                throw new InvalidOperationException("Não foi possível conectar ao servidor X11.");
            }

            _rootWindow = XDefaultRootWindow(_display);
            _cts = new CancellationTokenSource();
            _eventThread = new Thread(EventLoop)
            {
                IsBackground = true,
                Name = "X11HotkeyLoop"
            };
            _eventThread.Start();
        }
    }

    public void RegisterHotkey(HotkeyInfo hotkeyInfo)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        if (hotkeyInfo == null || !hotkeyInfo.IsValidHotkey) return;

        if (_display == IntPtr.Zero)
        {
            Initialize();
        }

        if (hotkeyInfo.ID == 0)
        {
            hotkeyInfo.ID = GenerateUniqueId();
        }

        int keycode;
        uint modifiers;

        lock (_x11Lock)
        {
            keycode = XKeysymToKeycode(_display, (IntPtr)MapToX11Keysym(hotkeyInfo.KeyCode));
            if (keycode == 0)
            {
                hotkeyInfo.Status = HotkeyStatus.Failed;
                return;
            }

            modifiers = MapToX11Modifiers(hotkeyInfo);

            // No X11, agarramos combinações com NumLock (Mod2 = 0x10) e CapsLock (LockMask = 0x02) para evitar bloqueios
            uint[] modifierMasks = [modifiers, modifiers | 0x10 /* NumLock */, modifiers | 0x02 /* CapsLock */, modifiers | 0x12];
            foreach (uint mask in modifierMasks)
            {
                XGrabKey(_display, keycode, mask, _rootWindow, true, 1 /* GrabModeAsync */, 1 /* GrabModeAsync */);
            }

            XFlush(_display);
        }

        _registeredHotkeys[(keycode, modifiers)] = hotkeyInfo;
        hotkeyInfo.Status = HotkeyStatus.Registered;
    }

    public bool UnregisterHotkey(HotkeyInfo hotkeyInfo)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        if (hotkeyInfo == null || _display == IntPtr.Zero) return false;

        int keycode;
        uint modifiers;

        lock (_x11Lock)
        {
            keycode = XKeysymToKeycode(_display, (IntPtr)MapToX11Keysym(hotkeyInfo.KeyCode));
            modifiers = MapToX11Modifiers(hotkeyInfo);

            uint[] modifierMasks = [modifiers, modifiers | 0x10, modifiers | 0x02, modifiers | 0x12];
            foreach (uint mask in modifierMasks)
            {
                XUngrabKey(_display, keycode, mask, _rootWindow);
            }

            XFlush(_display);
        }

        _registeredHotkeys.TryRemove((keycode, modifiers), out _);
        hotkeyInfo.ID = 0;
        hotkeyInfo.Status = HotkeyStatus.NotConfigured;
        return true;
    }

    private void EventLoop()
    {
        while (_cts?.IsCancellationRequested == false && _display != IntPtr.Zero)
        {
            bool hasEvent = false;
            XEvent ev = default;

            lock (_x11Lock)
            {
                if (_display != IntPtr.Zero && XPending(_display) > 0)
                {
                    XNextEvent(_display, out ev);
                    hasEvent = true;
                }
            }

            if (hasEvent)
            {
                if (ev.type == 2 /* KeyPress */)
                {
                    // Mascara NumLock (0x10) e CapsLock (0x02) para encontrar a combinação de modificadores correspondente
                    uint stateWithoutLocks = ev.xkey.state & ~(0x02u | 0x10u);

                    if (_registeredHotkeys.TryGetValue((ev.xkey.keycode, stateWithoutLocks), out HotkeyInfo info) ||
                        _registeredHotkeys.TryGetValue((ev.xkey.keycode, ev.xkey.state), out info))
                    {
                        if (CheckRepeatLimitTime())
                        {
                            Dispatcher.UIThread.Post(() =>
                            {
                                HotkeyPress?.Invoke(info.ID, info.KeyCode, info.ModifiersEnum);
                            });
                        }
                    }
                }
            }
            else
            {
                Thread.Sleep(10);
            }
        }
    }

    private bool CheckRepeatLimitTime()
    {
        if (HotkeyRepeatLimit > 0)
        {
            if (_repeatLimitTimer.ElapsedMilliseconds < HotkeyRepeatLimit) return false;
            _repeatLimitTimer.Restart();
        }

        return true;
    }

    private ushort GenerateUniqueId()
    {
        lock (_idLock)
        {
            do
            {
                _nextId++;
                if (_nextId == 0) _nextId = 1;
            } while (_registeredHotkeys.Values.Any(h => h.ID == _nextId));

            return _nextId;
        }
    }

    private static uint MapToX11Modifiers(HotkeyInfo hotkey)
    {
        uint mod = 0;
        if (hotkey.Shift) mod |= 0x01; // ShiftMask
        if (hotkey.Control) mod |= 0x04; // ControlMask
        if (hotkey.Alt) mod |= 0x08; // Mod1Mask (Alt)
        if (hotkey.Win) mod |= 0x40; // Mod4Mask (Super/Windows)
        return mod;
    }

    private static int MapToX11Keysym(InputKey key)
    {
        // Mapeamento de letras A-Z para Keysyms Latin minúsculos
        if (key >= InputKey.A && key <= InputKey.Z)
        {
            return 0x61 + (key - InputKey.A);
        }

        // Mapeamento de números 0-9
        if (key >= InputKey.D0 && key <= InputKey.D9)
        {
            return 0x30 + (key - InputKey.D0);
        }

        // Teclas de função F1..F12
        if (key >= InputKey.F1 && key <= InputKey.F12)
        {
            return 0xFFBE + (key - InputKey.F1);
        }

        // Teclas de função F13..F24
        if (key >= InputKey.F13 && key <= InputKey.F24)
        {
            return 0xFFCA + (key - InputKey.F13);
        }

        // Teclado numérico 0-9
        if (key >= InputKey.NumPad0 && key <= InputKey.NumPad9)
        {
            return 0xFFB0 + (key - InputKey.NumPad0);
        }

        // Mapeamento de teclas especiais comuns
        return key switch
        {
            InputKey.Back => 0xFF08,        // XK_BackSpace
            InputKey.Tab => 0xFF09,         // XK_Tab
            InputKey.LineFeed => 0xFF0A,    // XK_Linefeed
            InputKey.Clear => 0xFF0B,       // XK_Clear
            InputKey.Return => 0xFF0D,      // XK_Return
            InputKey.Pause => 0xFF13,       // XK_Pause
            InputKey.Scroll => 0xFF14,      // XK_Scroll_Lock
            InputKey.Escape => 0xFF1B,      // XK_Escape
            InputKey.Space => 0x0020,       // XK_space
            InputKey.Prior => 0xFF55,       // XK_Page_Up
            InputKey.Next => 0xFF56,        // XK_Page_Down
            InputKey.End => 0xFF57,         // XK_End
            InputKey.Home => 0xFF50,        // XK_Home
            InputKey.Left => 0xFF51,        // XK_Left
            InputKey.Up => 0xFF52,          // XK_Up
            InputKey.Right => 0xFF53,       // XK_Right
            InputKey.Down => 0xFF54,        // XK_Down
            InputKey.Select => 0xFF60,      // XK_Select
            InputKey.Print => 0xFF61,       // XK_Print
            InputKey.Execute => 0xFF62,     // XK_Execute
            InputKey.PrintScreen => 0xFF61, // XK_Print
            InputKey.Insert => 0xFF63,      // XK_Insert
            InputKey.Delete => 0xFFFF,      // XK_Delete
            InputKey.Help => 0xFF6A,        // XK_Help
            InputKey.Multiply => 0xFFAA,    // XK_KP_Multiply
            InputKey.Add => 0xFFAB,         // XK_KP_Add
            InputKey.Separator => 0xFFAC,   // XK_KP_Separator
            InputKey.Subtract => 0xFFAD,    // XK_KP_Subtract
            InputKey.Decimal => 0xFFAE,     // XK_KP_Decimal
            InputKey.Divide => 0xFFAF,      // XK_KP_Divide
            _ => (int)key
        };
    }

    public void Close() => Dispose();

    public void Dispose()
    {
        if (IsDisposed) return;
        IsDisposed = true;

        _cts?.Cancel();

        try
        {
            if (_eventThread != null && _eventThread.IsAlive)
            {
                _eventThread.Join(200);
            }
        }
        catch
        {
        }

        Closed?.Invoke(this, EventArgs.Empty);

        lock (_x11Lock)
        {
            if (_display != IntPtr.Zero)
            {
                foreach (var entry in _registeredHotkeys)
                {
                    uint[] modifierMasks = [entry.Key.Modifiers, entry.Key.Modifiers | 0x10, entry.Key.Modifiers | 0x02, entry.Key.Modifiers | 0x12];
                    foreach (uint mask in modifierMasks)
                    {
                        XUngrabKey(_display, entry.Key.KeyCode, mask, _rootWindow);
                    }
                    entry.Value.ID = 0;
                    entry.Value.Status = HotkeyStatus.NotConfigured;
                }
                _registeredHotkeys.Clear();

                XFlush(_display);
                XCloseDisplay(_display);
                _display = IntPtr.Zero;
            }
        }

        _cts?.Dispose();
        _cts = null;
    }

    [DllImport("libX11.so.6")] private static extern IntPtr XOpenDisplay(string display);
    [DllImport("libX11.so.6")] private static extern IntPtr XDefaultRootWindow(IntPtr display);
    [DllImport("libX11.so.6")] private static extern int XGrabKey(IntPtr display, int keycode, uint modifiers, IntPtr window, bool owner, int pMode, int kMode);
    [DllImport("libX11.so.6")] private static extern int XUngrabKey(IntPtr display, int keycode, uint modifiers, IntPtr window);
    [DllImport("libX11.so.6")] private static extern int XKeysymToKeycode(IntPtr display, IntPtr keysym);
    [DllImport("libX11.so.6")] private static extern int XPending(IntPtr display);
    [DllImport("libX11.so.6")] private static extern int XNextEvent(IntPtr display, out XEvent ev);
    [DllImport("libX11.so.6")] private static extern int XFlush(IntPtr display);
    [DllImport("libX11.so.6")] private static extern int XCloseDisplay(IntPtr display);

    [StructLayout(LayoutKind.Explicit, Size = 192)]
    private struct XEvent
    {
        [FieldOffset(0)] public int type;
        [FieldOffset(0)] public XKeyEvent xkey;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct XKeyEvent
    {
        public int type;
        public ulong serial;
        public bool send_event;
        public IntPtr display;
        public IntPtr window;
        public IntPtr root;
        public IntPtr subwindow;
        public ulong time;
        public int x, y, x_root, y_root;
        public uint state;
        public int keycode;
        public bool same_screen;
    }
}

#else

using ShareX.HelpersLib;
using System;

namespace ShareX.HelpersLib.Input.Linux;

/// <summary>
/// Windows stub for X11HotkeyHost. Avoids P/Invoke dependencies on libX11.so.6 when compiling for Windows.
/// </summary>
public sealed class X11HotkeyHost : IHotkeyHost
{
    public event HotkeyEventHandler HotkeyPress;
    public event EventHandler Closed;
#pragma warning disable CS0067
    public event EventHandler<NativeWindowMessageEventArgs> NativeMessageReceived;
#pragma warning restore CS0067

    public IntPtr Handle => IntPtr.Zero;
    public bool IsDisposed => false;
    public int HotkeyRepeatLimit { get; set; } = 1000;

    public void Initialize() => throw new PlatformNotSupportedException("X11HotkeyHost is only supported on Linux/X11.");
    public void RegisterHotkey(HotkeyInfo hotkeyInfo) { }
    public bool UnregisterHotkey(HotkeyInfo hotkeyInfo) => false;
    public void Close() { }
    public void Dispose() { }
}

#endif
