using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenTabletDriver.Native.Linux.Xorg;
using OpenTabletDriver.Plugin.Platform.Display;

namespace OpenTabletDriver.Desktop.Interop.Display
{
    using static XLib;
    using static XRandr;

    public class XScreen : IVirtualScreen, IDisposable
    {
        public unsafe XScreen()
        {
            Display = XOpenDisplay(null);

            if (Display == IntPtr.Zero)
                throw new InvalidOperationException("Could not open X display");

            RootWindow = XDefaultRootWindow(Display);

            if (RootWindow == IntPtr.Zero)
                throw new InvalidOperationException("Could not get X root window");

            var monitors = GetXRandrDisplays();
            var primary = monitors.FirstOrDefault(d => d.Primary != 0);

            var displays = new List<IDisplay> { this };

            for (int index = 0; index < monitors.Length; index++)
            {
                var monitor = monitors[index];

                var display = new Interop.Display.Display(
                    monitor.Width,
                    monitor.Height,
                    new Vector2(monitor.X - primary.X, monitor.Y - primary.Y),
                    index + 1);

                displays.Add(display);
            }

            Displays = displays;
            Position = new Vector2(primary.X, primary.Y);
        }

        private IntPtr Display;
        private IntPtr RootWindow;

        public float Width => XDisplayWidth(Display, 0);

        public float Height => XDisplayHeight(Display, 0);

        public Vector2 Position { private set; get; }

        private unsafe XRRMonitorInfo[] GetXRandrDisplays()
        {
            var xRandrMonitors = XRRGetMonitors(Display, RootWindow, true, out var count);
            var monitors = new XRRMonitorInfo[count];

            for (int i = 0; i < count; i++)
                monitors[i] = xRandrMonitors[i];

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
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _isDisposed;

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (Display != IntPtr.Zero)
            {
                int result = XCloseDisplay(Display);
                Display = IntPtr.Zero;
            }
            RootWindow = IntPtr.Zero;
            _isDisposed = true;
        }

        ~XScreen() => Dispose(false);
    }
}
