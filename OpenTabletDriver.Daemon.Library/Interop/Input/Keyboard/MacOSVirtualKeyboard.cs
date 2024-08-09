using System;
using System.Collections.Generic;
using OpenTabletDriver.Native.MacOS;
using OpenTabletDriver.Native.MacOS.Input;
using OpenTabletDriver.Platform.Keyboard;

namespace OpenTabletDriver.Daemon.Interop.Input.Keyboard
{
    using static MacOS;

    public class MacOSVirtualKeyboard : IVirtualKeyboard
    {
        private readonly IKeyMapper _keyMapper;

        public MacOSVirtualKeyboard(IKeyMapper keysProvider)
        {
            _keyMapper = keysProvider;
        }

        private void KeyEvent(BindableKey key, bool isPress)
        {
            var code = (CGKeyCode)_keyMapper[key];
            var keyEvent = CGEventCreateKeyboardEvent(IntPtr.Zero, code, isPress);
            CGEventPost(CGEventTapLocation.kCGHIDEventTap, keyEvent);
            CFRelease(keyEvent);
        }

        public void Press(BindableKey key)
        {
            KeyEvent(key, true);
        }

        public void Release(BindableKey key)
        {
            KeyEvent(key, false);
        }

        public void Press(IEnumerable<BindableKey> keys)
        {
            foreach (var key in keys)
                KeyEvent(key, true);
        }

        public void Release(IEnumerable<BindableKey> keys)
        {
            foreach (var key in keys)
                KeyEvent(key, false);
        }

        public IEnumerable<BindableKey> SupportedKeys => _keyMapper.GetBindableKeys();
    }
}
