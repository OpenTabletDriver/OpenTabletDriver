using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenTabletDriver.Native.Windows;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Platform.Display;

namespace OpenTabletDriver.Interop.Display
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

            var monitors = GetDisplays().OrderBy(e => e.Left).ToList();
            var primary = monitors.FirstOrDefault(m => m.IsPrimary);

            var displays = new List<IDisplay>();
            displays.Add(this);
            foreach (var monitor in monitors)
            {
                var display = new Display(
                    monitor.Width,
                    monitor.Height,
                    new Vector2(monitor.Left, monitor.Top),
                    monitors.IndexOf(monitor) + 1);
                displays.Add(display);
            }

            var x = primary.Left - monitors.Min(m => m.Left);
            var y = primary.Top - monitors.Min(m => m.Top);

            Displays = displays;
            Position = new Vector2(x, y);
        }

        private static List<DisplayInfo> GetDisplays()
        {
            List<DisplayInfo> displayCollection = new List<DisplayInfo>();
            MonitorEnumDelegate monitorDelegate = delegate (IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor,  IntPtr dwData)
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

        private IEnumerable<DisplayInfo> InternalDisplays => GetDisplays().OrderBy(e => e.Left);

        public float Width
        {
            get
            {
                var left = InternalDisplays.Min(d => d.Left);
                var right = InternalDisplays.Max(d => d.Right);
                return right - left;
            }
        }

        public float Height
        {
            get
            {
                var top = InternalDisplays.Min(d => d.Top);
                var bottom = InternalDisplays.Max(d => d.Bottom);
                return bottom - top;
            }
        }

        public Vector2 Position { private set; get; }

        public IEnumerable<IDisplay> Displays { private set; get; }

        public int Index => 0;

        public override string ToString()
        {
            return $"Virtual Display {Index} ({Width}x{Height}@{Position})";
        }
    }
}