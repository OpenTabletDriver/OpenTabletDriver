using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using NativeLib.Linux;
using TabletDriverPlugin;

namespace TabletDriverLib.Interop.Display
{
    using static XLib;
    using static XRandr;
    using Display = IntPtr;
    using Window = IntPtr;

    public class XScreen : IVirtualScreen, IDisposable
    {
        public unsafe XScreen()
        {
            Display = XOpenDisplay(null);
            RootWindow = XDefaultRootWindow(Display);

            var monitors = GetXRandrDisplays().ToList();
            var primary = monitors.FirstOrDefault(d => d.Primary != 0);

            var displays = new List<IDisplay>();
            displays.Add(this);
            foreach (var monitor in monitors)
            {
                var display = new Interop.Display.Display(
                    monitor.Width,
                    monitor.Height,
                    new Point(monitor.X - primary.X, monitor.Y - primary.Y),
                    monitors.IndexOf(monitor) + 1);
                displays.Add(display);
            }

            Displays = displays;
            Position = new Point(primary.X, primary.Y);
        }

        private Window Display;
        private Window RootWindow;

        public float Width
        {
            get => XDisplayWidth(Display, 0);
        }

        public float Height
        {
            get => XDisplayHeight(Display, 0);
        }

        public Point Position { private set; get; } = new Point(0, 0);

        private unsafe IEnumerable<XRRMonitorInfo> GetXRandrDisplays()
        {
            ICollection<XRRMonitorInfo> monitors = new List<XRRMonitorInfo>();
            var xRandrMonitors = XRRGetMonitors(Display, RootWindow, true, out var count);
            for (int i = 0; i < count; i++)
                monitors.Add(xRandrMonitors[i]);
            return monitors;
        }

        public IEnumerable<IDisplay> Displays { private set; get; }

        public int Index => 0;

        public override string ToString()
        {
            return $"X Screen {Index} ({Width}x{Height}@{Position})";
        }

        public void Dispose()
        {
            XCloseDisplay(Display);
        }
    }
}