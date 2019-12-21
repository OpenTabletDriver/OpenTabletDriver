using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using NativeLib.Linux;
using TabletDriverLib.Component;

namespace TabletDriverLib.Interop.Display
{
    using static XLib;
    using static XRandr;
    using Display = IntPtr;
    using Window = IntPtr;

    public class XDisplay : IVirtualScreen, IDisposable
    {
        public unsafe XDisplay()
        {
            Display = XOpenDisplay(null);
            RootWindow = XDefaultRootWindow(Display);
            var primary = GetXRandrDisplays().FirstOrDefault(d => d.Primary != 0);
            Position = new Point(primary.X, primary.Y);
        }

        private Display Display;
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

        public IEnumerable<IDisplay> Displays
        {
            get
            {
                var monitors = GetXRandrDisplays();
                ICollection<IDisplay> displays = new List<IDisplay>();
                var primary = monitors.FirstOrDefault(d => d.Primary != 0);
                foreach (var monitor in monitors)
                {
                    var pos = new Point(monitor.X - primary.X, monitor.Y - primary.Y);
                    var display = new ManualDisplay(monitor.Width, monitor.Height, pos);
                    displays.Add(display);
                }
                return displays;
            }
        }

        public void Dispose()
        {
            XCloseDisplay(Display);
        }
    }
}