using System;

namespace TabletDriverLib.Interop.Display
{
    public class XDisplay : IDisplay, IDisposable
    {
        public unsafe XDisplay()
        {
            Display = Native.Linux.XOpenDisplay(null);
            RootWindow = Native.Linux.XDefaultRootWindow(Display);
        }

        private IntPtr Display;
        private IntPtr RootWindow;

        public float Width
        {
            get => Native.Linux.XDisplayWidth(Display, 0);
        }

        public float Height
        {
            get => Native.Linux.XDisplayHeight(Display, 0);
        }

        public void Dispose()
        {
            Native.Linux.XCloseDisplay(Display);
        }
    }
}