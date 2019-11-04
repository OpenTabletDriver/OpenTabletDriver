using NativeLib.OSX;

namespace TabletDriverLib.Interop.Display
{
    using static OSX;

    public class MacOSDisplay : IDisplay
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
    }
}