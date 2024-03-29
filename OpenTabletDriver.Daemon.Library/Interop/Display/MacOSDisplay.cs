using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenTabletDriver.Native.MacOS;
using OpenTabletDriver.Native.MacOS.Generic;
using OpenTabletDriver.Platform.Display;

namespace OpenTabletDriver.Daemon.Interop.Display
{
    using static MacOS;

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

        public Vector2 Position => new Vector2(0, 0);

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
                       new Vector2(
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
