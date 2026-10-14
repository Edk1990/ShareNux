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
using System.Collections.Generic;
using System.Diagnostics;

namespace ShareX.HelpersLib.Input.Linux;

/// <summary>
/// Skeleton / stub for global hotkey management on Wayland compositors via XDG Desktop Portal (org.freedesktop.portal.GlobalShortcuts).
/// </summary>
public sealed class WaylandPortalHotkeyHost : IHotkeyHost
{
    private readonly ConcurrentDictionary<string, HotkeyInfo> _shortcuts = new();
    private readonly Stopwatch _repeatLimitTimer = Stopwatch.StartNew();

    public event HotkeyEventHandler HotkeyPress;
    public event EventHandler Closed;
#pragma warning disable CS0067
    public event EventHandler<NativeWindowMessageEventArgs> NativeMessageReceived;
#pragma warning restore CS0067

    public IntPtr Handle => IntPtr.Zero;
    public bool IsDisposed { get; private set; }
    public int HotkeyRepeatLimit { get; set; } = 1000;

    public void Initialize()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        Debug.WriteLine("WaylandPortalHotkeyHost initialized via XDG Desktop Portal.");
    }

    public void RegisterHotkey(HotkeyInfo hotkeyInfo)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        if (hotkeyInfo == null || !hotkeyInfo.IsValidHotkey) return;

        if (hotkeyInfo.ID == 0)
        {
            hotkeyInfo.ID = (ushort)(_shortcuts.Count + 1);
        }

        string shortcutId = $"sharex_action_{hotkeyInfo.ID}";
        _shortcuts[shortcutId] = hotkeyInfo;

        string accelerator = FormatPortalAccelerator(hotkeyInfo);
        RegisterViaDbusPortal(shortcutId, accelerator, hotkeyInfo.ToString());

        hotkeyInfo.Status = HotkeyStatus.Registered;
    }

    public bool UnregisterHotkey(HotkeyInfo hotkeyInfo)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        if (hotkeyInfo == null) return false;

        string shortcutId = $"sharex_action_{hotkeyInfo.ID}";
        _shortcuts.TryRemove(shortcutId, out _);
        hotkeyInfo.ID = 0;
        hotkeyInfo.Status = HotkeyStatus.NotConfigured;
        return true;
    }

    internal void OnPortalShortcutActivated(string shortcutId)
    {
        if (_shortcuts.TryGetValue(shortcutId, out HotkeyInfo info))
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

    private bool CheckRepeatLimitTime()
    {
        if (HotkeyRepeatLimit > 0)
        {
            if (_repeatLimitTimer.ElapsedMilliseconds < HotkeyRepeatLimit) return false;
            _repeatLimitTimer.Restart();
        }

        return true;
    }

    private static string FormatPortalAccelerator(HotkeyInfo hotkey)
    {
        List<string> parts = new();
        if (hotkey.Control) parts.Add("Control");
        if (hotkey.Alt) parts.Add("Alt");
        if (hotkey.Shift) parts.Add("Shift");
        if (hotkey.Win) parts.Add("Super");
        parts.Add(hotkey.KeyCode.ToString());
        return string.Join("+", parts);
    }

    private void RegisterViaDbusPortal(string id, string accelerator, string description)
    {
        Debug.WriteLine($"[WaylandPortal] Registering portal shortcut: id={id}, accelerator={accelerator}, description={description}");
    }

    public void Close() => Dispose();

    public void Dispose()
    {
        if (IsDisposed) return;
        IsDisposed = true;

        _shortcuts.Clear();
        Closed?.Invoke(this, EventArgs.Empty);
    }
}

#else

using ShareX.HelpersLib;
using System;

namespace ShareX.HelpersLib.Input.Linux;

/// <summary>
/// Windows stub for WaylandPortalHotkeyHost.
/// </summary>
public sealed class WaylandPortalHotkeyHost : IHotkeyHost
{
    public event HotkeyEventHandler HotkeyPress;
    public event EventHandler Closed;
#pragma warning disable CS0067
    public event EventHandler<NativeWindowMessageEventArgs> NativeMessageReceived;
#pragma warning restore CS0067

    public IntPtr Handle => IntPtr.Zero;
    public bool IsDisposed => false;
    public int HotkeyRepeatLimit { get; set; } = 1000;

    public void Initialize() => throw new PlatformNotSupportedException("WaylandPortalHotkeyHost is only supported on Linux/Wayland.");
    public void RegisterHotkey(HotkeyInfo hotkeyInfo) { }
    public bool UnregisterHotkey(HotkeyInfo hotkeyInfo) => false;
    public void Close() { }
    public void Dispose() { }
}

#endif
