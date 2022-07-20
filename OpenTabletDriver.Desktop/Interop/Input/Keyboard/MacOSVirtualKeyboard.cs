using System;
using System.Collections.Generic;
using OpenTabletDriver.Native.OSX;
using OpenTabletDriver.Native.OSX.Generic;
using OpenTabletDriver.Native.OSX.Input;
using OpenTabletDriver.Platform.Keyboard;

namespace OpenTabletDriver.Desktop.Interop.Input.Keyboard
{
    using static OSX;

    public class MacOSVirtualKeyboard : IVirtualKeyboard
    {
        private readonly IKeysProvider _keysProvider;

        public MacOSVirtualKeyboard(IKeysProvider keysProvider)
        {
            _keysProvider = keysProvider;
        }

        private void KeyEvent(string key, bool isPress)
        {
            if (_keysProvider.EtoToNative.TryGetValue(key, out var code))
            {
                var keyEvent = CGEventCreateKeyboardEvent(IntPtr.Zero, (CGKeyCode)code, isPress);
                CGEventPost(CGEventTapLocation.kCGHIDEventTap, keyEvent);
                CFRelease(keyEvent);
            }
        }

        public void Press(string key)
        {
            KeyEvent(key, true);
        }

        public void Release(string key)
        {
            KeyEvent(key, false);
        }

        public void Press(IEnumerable<string> keys)
        {
            foreach (var key in keys)
                KeyEvent(key, true);
        }

        public void Release(IEnumerable<string> keys)
        {
            foreach (var key in keys)
                KeyEvent(key, false);
        }

        public IEnumerable<string> SupportedKeys => _keysProvider.EtoToNative.Keys;
    }
}
