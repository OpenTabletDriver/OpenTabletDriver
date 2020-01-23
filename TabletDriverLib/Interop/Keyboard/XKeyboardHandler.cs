using System;
using NativeLib.Linux;

namespace TabletDriverLib.Interop.Keyboard
{
    using static XLib;

    using IntPtr = IntPtr;
    using Display = IntPtr;
    using Window = IntPtr;

    public class XKeyboardHandler : IKeyboardHandler, IDisposable
    {
        public unsafe XKeyboardHandler()
        {
            Display = XOpenDisplay(null);
        }

        private Display Display;

        private void KeyPress(string key, bool isPress)
        {
            var lowercase = key.ToLower();
            var keySym = XStringToKeysym(key);
            var keyCode = XKeysymToKeycode(Display, keySym);
            XTestFakeKeyEvent(Display, keyCode, isPress, 0UL);
            XFlush(Display);
            TabletDriverPlugin.Log.Debug($"Set keystate for key '{key}'({keyCode}) {(isPress ? "down" : "up")}");
        }

        public void Press(string key)
        {
            KeyPress(key, true);
        }

        public void Release(string key)
        {
            KeyPress(key, false);
        }

        public void Dispose()
        {
            XCloseDisplay(Display);
        }
    }
}