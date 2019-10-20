namespace TabletDriverLib.Interop.Display
{
    public class MacOSDisplay : IDisplay
    {
        private uint MainDisplay => Native.MacOSX.CGMainDisplayID();

        public float Width 
        {
            get => Native.MacOSX.CGDisplayPixelsWide(MainDisplay);
        }

        public float Height
        {
            get => Native.MacOSX.CGDisplayPixelsHigh(MainDisplay);
        }
    }
}