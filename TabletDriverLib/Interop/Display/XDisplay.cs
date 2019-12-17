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

        public Point Position => new Point(0, 0);

        public unsafe IEnumerable<IDisplay> Displays
        {
            get
            {
                ICollection<IDisplay> displays = new List<IDisplay>();
                var xRandrMonitors = XRRGetMonitors(Display, RootWindow, true, out var count);
                for (int i = 0; i < count; i++)
                {
                    var monitor = xRandrMonitors[i];
                    displays.Add(new ManualDisplay(
                        monitor.Width,
                        monitor.Height,
                        new Point(monitor.X, monitor.Y)));
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