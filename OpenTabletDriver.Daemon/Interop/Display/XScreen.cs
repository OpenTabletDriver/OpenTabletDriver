using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenTabletDriver.Native.Linux.Xorg;
using OpenTabletDriver.Platform.Display;

namespace OpenTabletDriver.Daemon.Interop.Display
{
    using static XLib;
    using static XRandr;
    using Window = IntPtr;

    public sealed class XScreen : IVirtualScreen, IDisposable
    {
        public unsafe XScreen()
        {
            _display = XOpenDisplay(null);
            _rootWindow = XDefaultRootWindow(_display);

            var monitors = GetXRandrDisplays().ToList();
            var primary = monitors.FirstOrDefault(d => d.Primary != 0);

            var displays = new List<IDisplay>();
            displays.Add(this);
            foreach (var monitor in monitors)
            {
                var display = new Display(
                    monitor.Width,
                    monitor.Height,
                    new Vector2(monitor.X - primary.X, monitor.Y - primary.Y),
                    monitors.IndexOf(monitor) + 1);
                displays.Add(display);
            }

            Displays = displays;
            Position = new Vector2(primary.X, primary.Y);
        }

        private readonly Window _display;
        private readonly Window _rootWindow;

        public float Width
        {
            get => XDisplayWidth(_display, 0);
        }

        public float Height
        {
            get => XDisplayHeight(_display, 0);
        }

        public Vector2 Position { get; }

        private unsafe IEnumerable<XRRMonitorInfo> GetXRandrDisplays()
        {
            ICollection<XRRMonitorInfo> monitors = new List<XRRMonitorInfo>();
            var xRandrMonitors = XRRGetMonitors(_display, _rootWindow, true, out var count);
            for (int i = 0; i < count; i++)
                monitors.Add(xRandrMonitors[i]);
            return monitors;
        }

        public IEnumerable<IDisplay> Displays { get; }

        public int Index => 0;

        public override string ToString()
        {
            return $"X Screen {Index} ({Width}x{Height}@{Position})";
        }

        public void Dispose()
        {
            XCloseDisplay(_display);
        }
    }
}
