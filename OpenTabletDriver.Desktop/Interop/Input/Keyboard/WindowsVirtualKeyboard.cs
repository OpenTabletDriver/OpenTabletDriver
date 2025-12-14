using System;
using System.Collections.Generic;
using OpenTabletDriver.Native.Windows;
using OpenTabletDriver.Native.Windows.Input;
using OpenTabletDriver.Plugin.Platform.Keyboard;

namespace OpenTabletDriver.Desktop.Interop.Input.Keyboard
{
    using static OpenTabletDriver.Native.Windows.Windows;

    public class WindowsVirtualKeyboard : IVirtualKeyboard
    {
        private void KeyEvent(string key, bool isPress)
        {
            var vk = EtoKeysymToVK[key];
            var input = new INPUT
            {
                type = INPUT_TYPE.KEYBD_INPUT,
                U = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = (short)vk,
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

        internal static readonly Dictionary<string, VirtualKey> EtoKeysymToVK = new Dictionary<string, VirtualKey>
        {
            { "None", 0x00 },
            { "A", VirtualKey.VK_A },
            { "B", VirtualKey.VK_B },
            { "C", VirtualKey.VK_C },
            { "D", VirtualKey.VK_D },
            { "E", VirtualKey.VK_E },
            { "F", VirtualKey.VK_F },
            { "G", VirtualKey.VK_G },
            { "H", VirtualKey.VK_H },
            { "I", VirtualKey.VK_I },
            { "J", VirtualKey.VK_J },
            { "K", VirtualKey.VK_K },
            { "L", VirtualKey.VK_L },
            { "M", VirtualKey.VK_M },
            { "N", VirtualKey.VK_N },
            { "O", VirtualKey.VK_O },
            { "P", VirtualKey.VK_P },
            { "Q", VirtualKey.VK_Q },
            { "R", VirtualKey.VK_R },
            { "S", VirtualKey.VK_S },
            { "T", VirtualKey.VK_T },
            { "U", VirtualKey.VK_U },
            { "V", VirtualKey.VK_V },
            { "W", VirtualKey.VK_W },
            { "X", VirtualKey.VK_X },
            { "Y", VirtualKey.VK_Y },
            { "Z", VirtualKey.VK_Z },
            { "F1", VirtualKey.VK_F1 },
            { "F2", VirtualKey.VK_F2 },
            { "F3", VirtualKey.VK_F3 },
            { "F4", VirtualKey.VK_F4 },
            { "F5", VirtualKey.VK_F5 },
            { "F6", VirtualKey.VK_F6 },
            { "F7", VirtualKey.VK_F7 },
            { "F8", VirtualKey.VK_F8 },
            { "F9", VirtualKey.VK_F9 },
            { "F10", VirtualKey.VK_F10 },
            { "F11", VirtualKey.VK_F11 },
            { "F12", VirtualKey.VK_F12 },
            { "F13", VirtualKey.VK_F13 },
            { "F14", VirtualKey.VK_F14 },
            { "F15", VirtualKey.VK_F15 },
            { "F16", VirtualKey.VK_F16 },
            { "F17", VirtualKey.VK_F17 },
            { "F18", VirtualKey.VK_F18 },
            { "F19", VirtualKey.VK_F19 },
            { "F20", VirtualKey.VK_F20 },
            { "F21", VirtualKey.VK_F21 },
            { "F22", VirtualKey.VK_F22 },
            { "F23", VirtualKey.VK_F23 },
            { "F24", VirtualKey.VK_F24 },
            { "D0", VirtualKey.VK_0 },
            { "D1", VirtualKey.VK_1 },
            { "D2", VirtualKey.VK_2 },
            { "D3", VirtualKey.VK_3 },
            { "D4", VirtualKey.VK_4 },
            { "D5", VirtualKey.VK_5 },
            { "D6", VirtualKey.VK_6 },
            { "D7", VirtualKey.VK_7 },
            { "D8", VirtualKey.VK_8 },
            { "D9", VirtualKey.VK_9 },
            { "Minus", VirtualKey.VK_OEM_MINUS },
            { "Grave", VirtualKey.VK_OEM_3 },
            { "Insert", VirtualKey.VK_INSERT },
            { "Home", VirtualKey.VK_HOME },
            { "PageUp", VirtualKey.VK_PRIOR },
            { "PageDown", VirtualKey.VK_NEXT },
            { "Delete", VirtualKey.VK_DELETE },
            { "End", VirtualKey.VK_END },
            { "Divide", VirtualKey.VK_DIVIDE },
            { "Decimal", VirtualKey.VK_DECIMAL },
            { "Backspace", VirtualKey.VK_BACK },
            { "Up", VirtualKey.VK_UP },
            { "Down", VirtualKey.VK_DOWN },
            { "Left", VirtualKey.VK_LEFT },
            { "Right", VirtualKey.VK_RIGHT },
            { "Tab", VirtualKey.VK_TAB },
            { "Space", VirtualKey.VK_SPACE },
            { "CapsLock", VirtualKey.VK_CAPITAL },
            { "ScrollLock", VirtualKey.VK_SCROLL },
            { "PrintScreen", VirtualKey.VK_SNAPSHOT },
            { "NumberLock", VirtualKey.VK_NUMLOCK },
            { "Enter", VirtualKey.VK_RETURN },
            { "Escape", VirtualKey.VK_ESCAPE },
            { "Multiply", VirtualKey.VK_MULTIPLY },
            { "Add", VirtualKey.VK_ADD },
            { "Subtract", VirtualKey.VK_SUBTRACT },
            { "Help", VirtualKey.VK_HELP },
            { "Pause", VirtualKey.VK_PAUSE },
            { "Clear", VirtualKey.VK_CLEAR },
            { "KeypadEqual", VirtualKey.VK_OEM_NEC_EQUAL },
            { "Menu", VirtualKey.VK_MENU },
            { "Backslash", VirtualKey.VK_OEM_5 },
            { "Plus", VirtualKey.VK_OEM_PLUS },
            { "Equal", VirtualKey.VK_OEM_PLUS },
            { "Semicolon", VirtualKey.VK_OEM_1 },
            { "Quote", VirtualKey.VK_OEM_7 },
            { "Comma", VirtualKey.VK_OEM_COMMA },
            { "Period", VirtualKey.VK_OEM_PERIOD },
            { "ForwardSlash", VirtualKey.VK_OEM_2 },
            { "Slash", VirtualKey.VK_OEM_2 },
            { "RightBracket", VirtualKey.VK_OEM_6 },
            { "LeftBracket", VirtualKey.VK_OEM_4 },
            { "ContextMenu", VirtualKey.VK_APPS },
            { "Keypad0", VirtualKey.VK_NUMPAD0 },
            { "Keypad1", VirtualKey.VK_NUMPAD1 },
            { "Keypad2", VirtualKey.VK_NUMPAD2 },
            { "Keypad3", VirtualKey.VK_NUMPAD3 },
            { "Keypad4", VirtualKey.VK_NUMPAD4 },
            { "Keypad5", VirtualKey.VK_NUMPAD5 },
            { "Keypad6", VirtualKey.VK_NUMPAD6 },
            { "Keypad7", VirtualKey.VK_NUMPAD7 },
            { "Keypad8", VirtualKey.VK_NUMPAD8 },
            { "Keypad9", VirtualKey.VK_NUMPAD9 },
            { "LeftShift", VirtualKey.VK_LSHIFT },
            { "RightShift", VirtualKey.VK_RSHIFT },
            { "LeftControl", VirtualKey.VK_LCONTROL },
            { "RightControl", VirtualKey.VK_RCONTROL },
            { "LeftAlt", VirtualKey.VK_LMENU },
            { "RightAlt", VirtualKey.VK_RMENU },
            { "LeftApplication", VirtualKey.VK_LWIN },
            { "RightApplication", VirtualKey.VK_RWIN },
            { "Shift", VirtualKey.VK_SHIFT },
            { "Alt", VirtualKey.VK_MENU },
            { "Control", VirtualKey.VK_CONTROL },
            { "Application", VirtualKey.VK_LWIN },
            { "Mute", VirtualKey.VK_VOLUME_MUTE },
            { "VolumeDown", VirtualKey.VK_VOLUME_DOWN },
            { "VolumeUp", VirtualKey.VK_VOLUME_UP },
            { "PlayPause", VirtualKey.VK_MEDIA_PLAY_PAUSE },
            { "PreviousSong", VirtualKey.VK_MEDIA_PREV_TRACK },
            { "NextSong", VirtualKey.VK_MEDIA_NEXT_TRACK },
            { "StopSong", VirtualKey.VK_MEDIA_STOP },
        };
    }
}
