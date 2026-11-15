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
        public XScreen(string? xDisplay = null)
        {
            Display = XOpenDisplay(xDisplay);
            _displayId = xDisplay ?? "0";
            RootWindow = XDefaultRootWindow(Display);

            var monitors = GetXRandrDisplays().ToList();
            var primary = monitors.FirstOrDefault(d => d.Primary != 0);

            var displays = new List<IDisplay>();
            displays.Add(this);
            foreach (var monitor in monitors)
            {
                var display = new SerializedDisplay(
                    Width: monitor.Width,
                    Height: monitor.Height,
                    Position: new Vector2(monitor.X - primary.X, monitor.Y - primary.Y),
                    Index: monitors.IndexOf(monitor) + 1);
                displays.Add(display);
            }

            Displays = displays;
            Position = new Vector2(primary.X, primary.Y);
        }

        private readonly string _displayId;
        private IntPtr Display;
        private IntPtr RootWindow;

        public float Width
        {
            get => XDisplayWidth(Display, 0);
        }

        public float Height
        {
            get => XDisplayHeight(Display, 0);
        }

        public Vector2 Position { private set; get; } = new Vector2(0, 0);

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

        string IDisplay.Name => $"X Screen {_displayId}";

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
