using System;
using System.Collections.Generic;
using System.Linq;
using NativeLib.OSX;
using NativeLib.OSX.Generic;
using TabletDriverPlugin;
using TabletDriverPlugin.Platform.Display;

namespace TabletDriverLib.Interop.Display
{
    using static OSX;

    public class MacOSDisplay : IVirtualScreen
    {
        public float Width 
        {
            get
            {
                var left = Displays.Min(d => d.Position.X);
                var right = Displays.Max(d => d.Position.X + d.Width);
                return right - left;
            }
        }

        public float Height
        {
            get
            {
                var top = Displays.Min(d => d.Position.Y);
                var bottom = Displays.Max(d => d.Position.Y + d.Height);
                return bottom - top;
            }
        }

        public Point Position => new Point(0, 0);

        public IEnumerable<IDisplay> Displays => _displays.Value;

        public int Index => 0;

        public override string ToString()
        {
            return $"VirtualDisplay {Index} ({Width}x{Height}@{Position})";
        }

        private Lazy<IEnumerable<IDisplay>> _displays = new Lazy<IEnumerable<IDisplay>>(() =>
        {
            var displayBounds = GetDisplayBounds().ToList();
            var offsetX = displayBounds.Min(d => d.origin.x);
            var offsetY = displayBounds.Min(d => d.origin.y);

            return from display in displayBounds
                select new Display(
                    (float)display.size.width,
                    (float)display.size.height,
                    new Point(
                        (float)(display.origin.x - offsetX),
                        (float)(display.origin.y - offsetY)
                    ),
                    displayBounds.IndexOf(display)
                );
        });

        private static IEnumerable<CGRect> GetDisplayBounds()
        {
            var displayIdBuf = new uint[10];
            CGGetActiveDisplayList(10, displayIdBuf, out var count);
            var displayIds = displayIdBuf.Take((int)count);
            return from id in displayIds
                select CGDisplayBounds(id);
        }
    }
}