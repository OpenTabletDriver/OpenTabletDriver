using System;
using System.Collections.Generic;
using NativeLib.Windows;
using NativeLib.Windows.Input;

namespace TabletDriverLib.Interop.Keyboard
{
    using static Windows;

    public class WindowsKeyboardHandler : IKeyboardHandler
    {
        private void KeyPress(string key, bool isPress)
        {
            var vk = XKeysymToVK[key];
            var input = new INPUT
            {
                type = 1,
                U = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = vk,
                        wScan = 0,
                        dwFlags = isPress ? KEYEVENTF.KEYDOWN : KEYEVENTF.KEYUP,
                        time = 0,
                        dwExtraInfo = UIntPtr.Zero
                    }
                }
            };

            var inputs = new INPUT[] { input };
            SendInput((uint)inputs.Length, inputs, INPUT.Size);
        }

        public void Press(string key)
        {
            KeyPress(key, true);
        }

        public void Release(string key)
        {
            KeyPress(key, false);
        }

        public static readonly Dictionary<string, short> XKeysymToVK = new Dictionary<string, short>
        {
            { "BackSpace", 0x08 },
            { "Tab", 0x09 },
            { "Linefeed", 0x0D },
            { "Clear", 0x0C },
            { "Return", 0x0D },
            { "Pause", 0x13 },
            { "Scroll_Lock", 0x91 },
            { "Escape", 0x1B },
            { "Delete", 0x28 },
            { "Home", 0x24 },
            { "Left", 0x25 },
            { "Up", 0x26 },
            { "Right", 0x27 },
            { "Down", 0x28 },
            { "Prior", 0x21 },
            { "Page_Up", 0x21 },
            { "Next", 0x22 },
            { "Page_Down", 0x22 },
            { "End", 0x23 },
            { "Begin", 0x24 },
            { "Select", 0x29 },
            { "Print", 0x2C },
            { "Execute", 0x2B },
            { "Insert", 0x2D },
            { "Menu", 0x12 },
            { "Cancel", 0x03 },
            { "Help", 0x2F },
            { "Break", 0x03 },
            { "Num_Lock", 0x90 },
            { "KP_Space", 0x20 },
            { "KP_Tab", 0x09 },
            { "KP_Enter", 0x0D },
            { "KP_Home", 0x24 },
            { "KP_Left", 0x25 },
            { "KP_Up", 0x26 },
            { "KP_Right", 0x27 },
            { "KP_Down", 0x28 },
            { "KP_Prior", 0x21 },
            { "KP_Page_Up", 0x21 },
            { "KP_Next", 0x22 },
            { "KP_Page_Down", 0x22 },
            { "KP_End", 0x23 },
            { "KP_Begin", 0x24 },
            { "KP_Insert", 0x2D },
            { "KP_Delete", 0x2E },
            { "KP_Multiply", 0x6A },
            { "KP_Add", 0x6B },
            { "KP_Separator", 0x6C },
            { "KP_Subtract", 0x6D },
            { "KP_Decimal", 0x6E },
            { "KP_Divide", 0x6F },
            { "KP_0", 0x60 },
            { "KP_1", 0x61 },
            { "KP_2", 0x62 },
            { "KP_3", 0x63 },
            { "KP_4", 0x64 },
            { "KP_5", 0x65 },
            { "KP_6", 0x66 },
            { "KP_7", 0x67 },
            { "KP_8", 0x68 },
            { "KP_9", 0x69 },
            { "F1", 0x70 },
            { "F2", 0x71 },
            { "F3", 0x72 },
            { "F4", 0x73 },
            { "F5", 0x74 },
            { "F6", 0x75 },
            { "F7", 0x76 },
            { "F8", 0x77 },
            { "F9", 0x78 },
            { "F10", 0x79 },
            { "F11", 0x7A },
            { "F12", 0x7B },
            { "F13", 0x7C },
            { "F14", 0x7D },
            { "F15", 0x7E },
            { "F16", 0x7F },
            { "F17", 0x80 },
            { "F18", 0x81 },
            { "F19", 0x82 },
            { "F20", 0x83 },
            { "F21", 0x84 },
            { "F22", 0x85 },
            { "F23", 0x86 },
            { "F24", 0x87 },
            { "Shift_L", 0xA0 },
            { "Shift_R", 0xA1 },
            { "Control_L", 0xA2 },
            { "Control_R", 0xA3 },
            { "Caps_Lock", 0x14 },
            { "Super_L", 0x5B },
            { "Super_R", 0x5C },
            { "space", 0x20 },
            { "apostrophe", 0xDE },
            { "quoteright", 0xDE },
            { "plus", 0xBB },
            { "comma", 0xBC },
            { "minus", 0xBD },
            { "period", 0xBE },
            { "slash", 0xBF },
            { "0", 0x30 },
            { "1", 0x31 },
            { "2", 0x32 },
            { "3", 0x33 },
            { "4", 0x34 },
            { "5", 0x35 },
            { "6", 0x36 },
            { "7", 0x37 },
            { "8", 0x38 },
            { "9", 0x39 },
            { "colon", 0xBA },
            { "semicolon", 0xBA },
            { "question", 0xBF },
            { "A", 0x41 },
            { "B", 0x42 },
            { "C", 0x43 },
            { "D", 0x44 },
            { "E", 0x45 },
            { "F", 0x46 },
            { "G", 0x47 },
            { "H", 0x48 },
            { "I", 0x49 },
            { "J", 0x4A },
            { "K", 0x4B },
            { "L", 0x4C },
            { "M", 0x4D },
            { "N", 0x4E },
            { "O", 0x4F },
            { "P", 0x50 },
            { "Q", 0x51 },
            { "R", 0x52 },
            { "S", 0x53 },
            { "T", 0x54 },
            { "U", 0x55 },
            { "V", 0x56 },
            { "W", 0x57 },
            { "X", 0x58 },
            { "Y", 0x59 },
            { "Z", 0x5A },
            { "bracketleft", 0xDB },
            { "backslash", 0xDC },
            { "bracketright", 0xDD },
            { "underscore", 0xBD },
            { "grave", 0xC0 },
            { "quoteleft", 0xDE },
            { "a", 0x41 },
            { "b", 0x42 },
            { "c", 0x43 },
            { "d", 0x44 },
            { "e", 0x45 },
            { "f", 0x46 },
            { "g", 0x47 },
            { "h", 0x48 },
            { "i", 0x49 },
            { "j", 0x4A },
            { "k", 0x4B },
            { "l", 0x4C },
            { "m", 0x4D },
            { "n", 0x4E },
            { "o", 0x4F },
            { "p", 0x50 },
            { "q", 0x51 },
            { "r", 0x52 },
            { "s", 0x53 },
            { "t", 0x54 },
            { "u", 0x55 },
            { "v", 0x56 },
            { "w", 0x57 },
            { "x", 0x58 },
            { "y", 0x59 },
            { "z", 0x5A },
            { "braceleft", 0xDB },
            { "bar", 0xDC },
            { "braceright", 0xDD },
            { "asciitilde", 0xC0 },
        };
    }
}