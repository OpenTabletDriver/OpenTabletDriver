using System;
using System.Collections.Generic;
using OpenTabletDriver.Native.MacOS;
using OpenTabletDriver.Native.MacOS.Input;
using OpenTabletDriver.Platform.Keyboard;

namespace OpenTabletDriver.Daemon.Library.Interop.Input.Keyboard
{
    using static MacOS;

    public class MacOSVirtualKeyboard : IVirtualKeyboard
    {
        private readonly IKeyMapper _keyMapper;
        // Keep track of current modifier flags, as CGEventSourceFlagsState does not return updated flags immediately after an event is posted.
        private ulong _currentFlags = 0;

        public MacOSVirtualKeyboard(IKeyMapper keysProvider)
        {
            _keyMapper = keysProvider;
        }

        private void KeyEvent(BindableKey key, bool isPress)
        {
            var code = (CGKeyCode)_keyMapper[key];
            var keyEvent = CGEventCreateKeyboardEvent(IntPtr.Zero, code, isPress);
            var flag = fromCGKeyCode((CGKeyCode)code);
            if (flag != 0)
            {
                if (!isPress)
                {
                    _currentFlags &= ~(ulong)flag;
                }
                else
                {
                    _currentFlags |= (ulong)flag;
                }
            }
            CGEventSetFlags(keyEvent, _currentFlags);
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

        private CGEventFlags fromCGKeyCode(CGKeyCode code)
        {
            return code switch
            {
                CGKeyCode.kVK_CapsLock => CGEventFlags.kCGEventFlagMaskAlphaShift,
                CGKeyCode.kVK_Command or CGKeyCode.kVK_RightCommand => CGEventFlags.kCGEventFlagMaskCommand,
                CGKeyCode.kVK_Control or CGKeyCode.kVK_RightControl => CGEventFlags.kCGEventFlagMaskControl,
                CGKeyCode.kVK_Function => CGEventFlags.kCGEventFlagMaskSecondaryFn,
                CGKeyCode.kVK_Help => CGEventFlags.kCGEventFlagMaskHelp,
                CGKeyCode.kVK_Option or CGKeyCode.kVK_RightOption => CGEventFlags.kCGEventFlagMaskAlternate,
                CGKeyCode.kVK_Shift or CGKeyCode.kVK_RightShift => CGEventFlags.kCGEventFlagMaskShift,
                _ => 0
            };
        }
    }
}
