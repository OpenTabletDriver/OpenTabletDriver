using System;
using System.Collections.Generic;
using NativeLib.OSX;
using TabletDriverPlugin;
using TabletDriverPlugin.Platform.Display;

namespace TabletDriverLib.Interop.Display
{
    using static OSX;

    public class MacOSDisplay : IVirtualScreen
    {
        private uint MainDisplay => CGMainDisplayID();

        public float Width 
        {
            get => CGDisplayPixelsWide(MainDisplay);
        }

        public float Height
        {
            get => CGDisplayPixelsHigh(MainDisplay);
        }

        public Point Position => new Point(0, 0);

        public IEnumerable<IDisplay> Displays
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
                        CGDisplayPixelsWide(MainDisplay),
                        CGDisplayPixelsWide(MainDisplay),
                        new Point((float)bound.origin.x, (float)bound.origin.y),
                        i) ;

                    yield return display;
                }
                yield return this;
            }
        }

        public int Index => 0;

        public override string ToString()
        {
            return $"VirtualDisplay {Index} ({Width}x{Height}@{Position})";
        }
    }
}