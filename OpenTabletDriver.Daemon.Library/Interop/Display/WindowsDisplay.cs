using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenTabletDriver.Native.Windows;
using OpenTabletDriver.Platform.Display;

namespace OpenTabletDriver.Daemon.Library.Interop.Display
{
    using static Windows;

    public class WindowsDisplay : IVirtualScreen
    {
        public WindowsDisplay()
        {
            try
            {
                SetProcessDpiAwareness(2);
                Log.Debug("Display", "DPI Awareness enabled");
            }
            catch { }
        }

        private static List<DisplayInfo> GetDisplays()
        {
            List<DisplayInfo> displayCollection = new List<DisplayInfo>();
            MonitorEnumDelegate monitorDelegate = delegate (IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData)
            {
                MonitorInfoEx monitorInfo = new MonitorInfoEx();
                monitorInfo.size = (uint)Marshal.SizeOf(monitorInfo);
                if (GetMonitorInfo(hMonitor, ref monitorInfo))
                {
                    var info = new DevMode();
                    EnumDisplaySettings(monitorInfo.deviceName, -1, ref info);

                    var monitor = new Rect
                    {
                        left = info.dmPositionX,
                        right = info.dmPositionX + info.dmPelsWidth,
                        top = info.dmPositionY,
                        bottom = info.dmPositionY + info.dmPelsHeight
                    };

                    DisplayInfo displayInfo = new DisplayInfo(monitor, monitorInfo.flags);
                    displayCollection.Add(displayInfo);
                }
                return true;
            };
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, monitorDelegate, IntPtr.Zero);
            return displayCollection;
        }

        private static List<DisplayInfo> InternalDisplays => GetDisplays()
            .OrderBy(e => e.Left)
            .ThenBy(e => e.Top)
            .ToList();

        public float Width
        {
            get
            {
                var displays = InternalDisplays;
                if (displays.Count == 0)
                    return 0;

                var left = displays.Min(d => d.Left);
                var right = displays.Max(d => d.Right);
                return right - left;
            }
        }

        public float Height
        {
            get
            {
                var displays = InternalDisplays;
                if (displays.Count == 0)
                    return 0;

                var top = displays.Min(d => d.Top);
                var bottom = displays.Max(d => d.Bottom);
                return bottom - top;
            }
        }

        public Vector2 Position
        {
            get
            {
                var displays = InternalDisplays;
                if (displays.Count == 0)
                    return Vector2.Zero;

                var primary = displays.FirstOrDefault(m => m.IsPrimary) ?? displays[0];
                var x = primary.Left - displays.Min(m => m.Left);
                var y = primary.Top - displays.Min(m => m.Top);
                return new Vector2(x, y);
            }
        }

        public IEnumerable<IDisplay> Displays
        {
            get
            {
                var monitors = InternalDisplays;
                yield return this;

                for (var index = 0; index < monitors.Count; index++)
                {
                    var monitor = monitors[index];
                    yield return new Display(
                        monitor.Width,
                        monitor.Height,
                        new Vector2(monitor.Left, monitor.Top),
                        index + 1);
                }
            }
        }

        public int Index => 0;

        public override string ToString()
        {
            return $"Virtual Display {Index} ({Width}x{Height}@{Position})";
        }
    }
}
