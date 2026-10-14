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

using ShareX.HelpersLib;
#if !WINDOWS
using ShareX.HelpersLib.Input.Linux;
#endif
using System;

namespace ShareX.HelpersLib.Input;

/// <summary>
/// Factory to instantiate the appropriate platform-specific global hotkey host.
/// </summary>
public static class HotkeyHostFactory
{
    /// <summary>
    /// Creates and returns an <see cref="IHotkeyHost"/> implementation based on the current operating system and display server session.
    /// </summary>
    public static IHotkeyHost CreateHost()
    {
#if WINDOWS
        return new WindowsHotkeyHost();
#elif !WINDOWS
        if (OperatingSystem.IsWindows())
        {
            return new WindowsHotkeyHost();
        }

        if (OperatingSystem.IsLinux())
        {
            string sessionType = Environment.GetEnvironmentVariable("XDG_SESSION_TYPE")?.ToLowerInvariant();
            string waylandDisplay = Environment.GetEnvironmentVariable("WAYLAND_DISPLAY");

            bool isWayland = sessionType == "wayland" || !string.IsNullOrWhiteSpace(waylandDisplay);

            if (isWayland)
            {
                return new WaylandPortalHotkeyHost();
            }

            return new X11HotkeyHost();
        }

        throw new PlatformNotSupportedException("Sistema operacional não suportado para Global Hotkeys.");
#endif
    }

    /// <summary>
    /// Alias for <see cref="CreateHost"/>.
    /// </summary>
    public static IHotkeyHost Create() => CreateHost();
}
