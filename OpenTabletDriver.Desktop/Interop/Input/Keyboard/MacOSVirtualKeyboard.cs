using System;
using System.Collections.Generic;
using OpenTabletDriver.Native.MacOS;
using OpenTabletDriver.Native.MacOS.Generic;
using OpenTabletDriver.Native.MacOS.Input;
using OpenTabletDriver.Platform.Keyboard;

namespace OpenTabletDriver.Desktop.Interop.Input.Keyboard
{
    using static MacOS;

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
                var flag = fromCGKeyCode((CGKeyCode)code);
                var currentFlag = CGEventSourceFlagsState(CGEventSourceStateHIDSystemState) & (0xffffffff ^ 0x20000100);
                if (flag != 0)
                {
                    if (!isPress)
                    {
                        currentFlag &= ~(ulong)flag;
                    }
                    else
                    {
                        currentFlag |= (ulong)flag;
                    }
                }
                CGEventSetFlags(keyEvent, currentFlag);
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
