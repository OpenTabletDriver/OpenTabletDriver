using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenTabletDriver.Native.Linux.Xorg;
using OpenTabletDriver.Plugin.Platform.Display;

#nullable enable

namespace OpenTabletDriver.Desktop.Interop.Display
{
    using static XLib;
    using static XRandr;

    public class XScreen : IVirtualScreen, IDisposable
    {
        public XScreen()
        {
            _display = XOpenDisplay(null);
            _rootWindow = XDefaultRootWindow(_display);

            var monitors = GetXRandrDisplays();
            var primary = monitors.FirstOrDefault(d => d.Primary != 0);

            var displays = new List<IDisplay>();
            displays.Add(this);
            foreach (var monitor in monitors)
            {
                var display = new Interop.Display.Display(
                    monitor.Width,
                    monitor.Height,
                    new Vector2(monitor.X - primary.X, monitor.Y - primary.Y),
                    monitors.IndexOf(monitor) + 1);
                displays.Add(display);
            }

            Displays = displays;
            Position = new Vector2(primary.X, primary.Y);
        }

        private XLibDisplayHandle? _display;
        private XLibWindowHandle? _rootWindow;

        public float Width => XDisplayWidth(_display ?? throw new InvalidOperationException("Display unset"), 0);

        public float Height => XDisplayHeight(_display ?? throw new InvalidOperationException("Display unset"), 0);

        public Vector2 Position { private set; get; }

        private List<XRRMonitorInfo> GetXRandrDisplays() =>
            [..XRRGetMonitors(_display is { IsInvalid: false } ? _display : throw new InvalidOperationException("Invalid Display"),
                _rootWindow is { IsInvalid: false } ? _rootWindow : throw new InvalidOperationException("Invalid RootWindow"),
                true, out _)];

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

            if (_display is { IsInvalid: false })
                _display.Dispose();
            _display = null;

            if (_rootWindow is { IsInvalid: false })
                _rootWindow.Dispose();
            _rootWindow = null;

            _isDisposed = true;
        }

        ~XScreen() => Dispose(false);
    }
}
