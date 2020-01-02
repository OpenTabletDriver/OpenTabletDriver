using System;
using System.Collections.Generic;
using System.Linq;
using NativeLib.Windows;
using TabletDriverLib.Component;

namespace TabletDriverLib.Interop.Display
{
    using static Windows;

    public class WindowsDisplay : IVirtualScreen
    {
        public WindowsDisplay()
        {
            var monitors = GetDisplays().OrderBy(e => e.Left).ToList();
            var primary = monitors.FirstOrDefault(m => m.IsPrimary);

            var displays = new List<IDisplay>();
            displays.Add(this);
            foreach (var monitor in monitors)
            {
                var display = new ManualDisplay(
                    monitor.Width,
                    monitor.Height,
                    new Point(monitor.Left, monitor.Top),
                    monitors.IndexOf(monitor) + 1);
                displays.Add(display);
            }

            var x = primary.Left - monitors.Min(m => m.Left);
            var y = primary.Top - monitors.Min(m => m.Top);

            Displays = displays;
            Position = new Point(x, y);
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

        public Point Position { private set; get; }

        public IEnumerable<IDisplay> Displays { private set; get; }

        public int Index => 0;

        public override string ToString()
        {
            return $"Virtual Display {Index} ({Width}x{Height}@{Position})";
        }
    }
}