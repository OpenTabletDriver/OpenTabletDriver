using System.Collections.Generic;
using NativeLib.OSX;
using TabletDriverLib.Component;

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
                // TODO: Multiple display support
                yield return this;
            }
        }
    }
}