using System;
using NativeLib.Linux;

namespace TabletDriverLib.Interop.Display
{
    using Display = IntPtr;
    using Window = IntPtr;
    using static Linux;

    public class XDisplay : IDisplay, IDisposable
    {
        public unsafe XDisplay()
        {
            Display = XOpenDisplay(null);
            RootWindow = XDefaultRootWindow(Display);
        }

        private Display Display;
        private Window RootWindow;

        public float Width
        {
            get => XDisplayWidth(Display, 0);
        }

        public float Height
        {
            get => XDisplayHeight(Display, 0);
        }

        public void Dispose()
        {
            XCloseDisplay(Display);
        }
    }
}