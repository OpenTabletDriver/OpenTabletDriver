using System;
using System.Collections.Generic;
using System.Linq;
using NativeLib.OSX;
using TabletDriverPlugin;
using TabletDriverPlugin.Platform.Display;

namespace TabletDriverLib.Interop.Display
{
    using static OSX;

    public class MacOSDisplay : IVirtualScreen
    {
        public MacOSDisplay()
        {
            var monitors = InternalDisplays;
            var primary = monitors.First();
            var displays = new List<IDisplay>();
            displays.Add(this);
            foreach (var monitor in monitors)
            {
                displays.Add(monitor);
            }
            var x = primary.Position.X - monitors.Min(m => m.Position.X);
            var y = primary.Position.Y - monitors.Min(m => m.Position.Y);
            Displays = displays;
            Position = new Point(x, y);
        }

        private IEnumerable<Display> GetDisplays
        {
            get
            {
                var displayIDs = new uint[10];
                uint count = 0;
                CGGetActiveDisplayList(100, displayIDs, ref count);
                for (var i = 0; i < count; i++)
                {
                    var bound = CGDisplayBounds(displayIDs[i]);

                    var display = new Display(
                        CGDisplayPixelsWide(displayIDs[i]),
                        CGDisplayPixelsHigh(displayIDs[i]),
                        new Point((float)bound.origin.x, (float)bound.origin.y),
                        i);

                    yield return display;
                }
            }
        }

        private IEnumerable<Display> InternalDisplays => GetDisplays.ToList().OrderBy(e => e.Position.X);

        public float Width
        {
            get
            {
                var left = InternalDisplays.Min(d => d.Position.X);
                var right = InternalDisplays.Max(d => d.Position.X + d.Width);
                return right - left;
            }
        }

        public float Height
        {
            get
            {
                var top = InternalDisplays.Min(d => d.Position.Y);
                var bottom = InternalDisplays.Max(d => d.Position.Y + d.Height);
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