using System;
using System.Collections.Generic;
using OpenTabletDriver.Native.OSX;
using OpenTabletDriver.Native.OSX.Generic;
using OpenTabletDriver.Native.OSX.Input;
using OpenTabletDriver.Plugin.Platform.Keyboard;

namespace OpenTabletDriver.Desktop.Interop.Input.Keyboard
{
    using static OSX;

    public class MacOSVirtualKeyboard : IVirtualKeyboard
    {
        private void KeyEvent(string key, bool isPress)
        {
            if (EtoKeysymToVK.TryGetValue(key, out var code))
            {
                var keyEvent = CGEventCreateKeyboardEvent(IntPtr.Zero, code, isPress);
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

        public IEnumerable<string> SupportedKeys => EtoKeysymToVK.Keys;

        internal static readonly Dictionary<string, CGKeyCode> EtoKeysymToVK = new Dictionary<string, CGKeyCode>
        {
            { "None", 0x00 },
            { "A", CGKeyCode.kVK_ANSI_A },
            { "B", CGKeyCode.kVK_ANSI_B },
            { "C", CGKeyCode.kVK_ANSI_C },
            { "D", CGKeyCode.kVK_ANSI_D },
            { "E", CGKeyCode.kVK_ANSI_E },
            { "F", CGKeyCode.kVK_ANSI_F },
            { "G", CGKeyCode.kVK_ANSI_G },
            { "H", CGKeyCode.kVK_ANSI_H },
            { "I", CGKeyCode.kVK_ANSI_I },
            { "J", CGKeyCode.kVK_ANSI_J },
            { "K", CGKeyCode.kVK_ANSI_K },
            { "L", CGKeyCode.kVK_ANSI_L },
            { "M", CGKeyCode.kVK_ANSI_M },
            { "N", CGKeyCode.kVK_ANSI_N },
            { "O", CGKeyCode.kVK_ANSI_O },
            { "P", CGKeyCode.kVK_ANSI_P },
            { "Q", CGKeyCode.kVK_ANSI_Q },
            { "R", CGKeyCode.kVK_ANSI_R },
            { "S", CGKeyCode.kVK_ANSI_S },
            { "T", CGKeyCode.kVK_ANSI_T },
            { "U", CGKeyCode.kVK_ANSI_U },
            { "V", CGKeyCode.kVK_ANSI_V },
            { "W", CGKeyCode.kVK_ANSI_W },
            { "X", CGKeyCode.kVK_ANSI_X },
            { "Y", CGKeyCode.kVK_ANSI_Y },
            { "Z", CGKeyCode.kVK_ANSI_Z },
            { "F1", CGKeyCode.kVK_F1 },
            { "F2", CGKeyCode.kVK_F2 },
            { "F3", CGKeyCode.kVK_F3 },
            { "F4", CGKeyCode.kVK_F4 },
            { "F5", CGKeyCode.kVK_F5 },
            { "F6", CGKeyCode.kVK_F6 },
            { "F7", CGKeyCode.kVK_F7 },
            { "F8", CGKeyCode.kVK_F8 },
            { "F9", CGKeyCode.kVK_F9 },
            { "F10", CGKeyCode.kVK_F10 },
            { "F11", CGKeyCode.kVK_F11 },
            { "F12", CGKeyCode.kVK_F12 },
            { "D0", CGKeyCode.kVK_ANSI_0 },
            { "D1", CGKeyCode.kVK_ANSI_1 },
            { "D2", CGKeyCode.kVK_ANSI_2 },
            { "D3", CGKeyCode.kVK_ANSI_3 },
            { "D4", CGKeyCode.kVK_ANSI_4 },
            { "D5", CGKeyCode.kVK_ANSI_5 },
            { "D6", CGKeyCode.kVK_ANSI_6 },
            { "D7", CGKeyCode.kVK_ANSI_7 },
            { "D8", CGKeyCode.kVK_ANSI_8 },
            { "D9", CGKeyCode.kVK_ANSI_9 },
            { "Minus", CGKeyCode.kVK_ANSI_Minus },
            { "Grave", CGKeyCode.kVK_ANSI_Grave },
            //{ "Insert", CGKeyCode.kVK_ANSI_INSERT },
            { "Home", CGKeyCode.kVK_Home },
            { "PageUp", CGKeyCode.kVK_PageUp },
            { "PageDown", CGKeyCode.kVK_PageDown },
            { "Delete", CGKeyCode.kVK_ForwardDelete },
            { "End", CGKeyCode.kVK_End },
            { "Divide", CGKeyCode.kVK_ANSI_KeypadDivide },
            { "Decimal", CGKeyCode.kVK_ANSI_KeypadDecimal },
            { "Backspace", CGKeyCode.kVK_Delete },
            { "Up", CGKeyCode.kVK_UpArrow },
            { "Down", CGKeyCode.kVK_DownArrow },
            { "Left", CGKeyCode.kVK_LeftArrow },
            { "Right", CGKeyCode.kVK_RightArrow },
            { "Tab", CGKeyCode.kVK_Tab },
            { "Space", CGKeyCode.kVK_Space },
            { "CapsLock", CGKeyCode.kVK_CapsLock },
            //{ "ScrollLock", CGKeyCode.kVK_F14 },
            //{ "PrintScreen", CGKeyCode.kVK_F13 },
            { "NumberLock", CGKeyCode.kVK_ANSI_KeypadClear },
            { "Enter", CGKeyCode.kVK_Return },
            { "Escape", CGKeyCode.kVK_Escape },
            { "Multiply", CGKeyCode.kVK_ANSI_KeypadMultiply },
            { "Add", CGKeyCode.kVK_ANSI_KeypadPlus },
            { "Help", CGKeyCode.kVK_Help },
            //{ "Pause", CGKeyCode.kVK_F15 },
            { "Clear", CGKeyCode.kVK_ANSI_KeypadClear },
            { "KeypadEqual", CGKeyCode.kVK_ANSI_KeypadEquals },
            //{ "Menu", CGKeyCode.kVK_ANSI_MENU },
            { "Backslash", CGKeyCode.kVK_ANSI_Backslash },
            { "Plus", CGKeyCode.kVK_ANSI_KeypadPlus },
            { "Equal", CGKeyCode.kVK_ANSI_KeypadEquals },
            { "Semicolon", CGKeyCode.kVK_ANSI_Semicolon },
            { "Quote", CGKeyCode.kVK_ANSI_Quote },
            { "Comma", CGKeyCode.kVK_ANSI_Comma },
            { "Period", CGKeyCode.kVK_ANSI_Period },
            { "ForwardSlash", CGKeyCode.kVK_ANSI_Slash },
            { "Slash", CGKeyCode.kVK_ANSI_Backslash },
            { "RightBracket", CGKeyCode.kVK_ANSI_RightBracket },
            { "LeftBracket", CGKeyCode.kVK_ANSI_LeftBracket },
            //{ "ContextMenu", CGKeyCode.kVK_ANSI_MENU },
            { "Keypad0", CGKeyCode.kVK_ANSI_Keypad0 },
            { "Keypad1", CGKeyCode.kVK_ANSI_Keypad1 },
            { "Keypad2", CGKeyCode.kVK_ANSI_Keypad2 },
            { "Keypad3", CGKeyCode.kVK_ANSI_Keypad3 },
            { "Keypad4", CGKeyCode.kVK_ANSI_Keypad4 },
            { "Keypad5", CGKeyCode.kVK_ANSI_Keypad5 },
            { "Keypad6", CGKeyCode.kVK_ANSI_Keypad6 },
            { "Keypad7", CGKeyCode.kVK_ANSI_Keypad7 },
            { "Keypad8", CGKeyCode.kVK_ANSI_Keypad8 },
            { "Keypad9", CGKeyCode.kVK_ANSI_Keypad9 },
            { "LeftShift", CGKeyCode.kVK_Shift },
            { "RightShift", CGKeyCode.kVK_RightShift },
            { "LeftControl", CGKeyCode.kVK_Control },
            { "RightControl", CGKeyCode.kVK_RightControl },
            { "LeftAlt", CGKeyCode.kVK_Command },
            { "RightAlt", CGKeyCode.kVK_RightCommand },
            { "LeftApplication", CGKeyCode.kVK_Option },
            { "RightApplication", CGKeyCode.kVK_RightOption },
            { "Shift", CGKeyCode.kVK_Shift },
            { "Alt", CGKeyCode.kVK_Command },
            { "Control", CGKeyCode.kVK_Control },
            { "Application", CGKeyCode.kVK_Option },
        };
    }
}
