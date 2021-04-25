using System;
using System.Collections.Generic;
using System.Linq;
using OpenTabletDriver.Native.Linux;
using OpenTabletDriver.Native.Linux.Evdev;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Platform.Keyboard;

namespace OpenTabletDriver.Desktop.Interop.Input.Keyboard
{
    public class EvdevVirtualKeyboard : IVirtualKeyboard, IDisposable
    {
        public EvdevVirtualKeyboard()
        {
            Device = new EvdevDevice("OpenTabletDriver Virtual Keyboard");

            Device.EnableTypeCodes(EventType.EV_KEY, EtoKeysymToEventCode.Values.Distinct().ToArray());

            var result = Device.Initialize();
            switch (result)
            {
                case ERRNO.NONE:
                    Log.Debug("Evdev", $"Successfully initialized virtual keyboard. (code {result})");
                    break;
                default:
                    Log.Write("Evdev", $"Failed to initialize virtual keyboard. (error code {result})", LogLevel.Error);
                    break;
            }
        }

        private EvdevDevice Device { set; get; }

        private void KeyEvent(string key, bool isPress)
        {
            var keyEventCode = EtoKeysymToEventCode[key];

            Device.Write(EventType.EV_KEY, keyEventCode, isPress ? 1 : 0);
            Device.Sync();
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

        public void Dispose()
        {
            Device?.Dispose();
        }

        public IEnumerable<string> SupportedKeys => EtoKeysymToEventCode.Keys;

        internal static readonly Dictionary<string, EventCode> EtoKeysymToEventCode = new Dictionary<string, EventCode>
        {
            { "None", 0x0 },
            { "A", EventCode.KEY_A },
            { "B", EventCode.KEY_B },
            { "C", EventCode.KEY_C },
            { "D", EventCode.KEY_D },
            { "E", EventCode.KEY_E },
            { "F", EventCode.KEY_F },
            { "G", EventCode.KEY_G },
            { "H", EventCode.KEY_H },
            { "I", EventCode.KEY_I },
            { "J", EventCode.KEY_J },
            { "K", EventCode.KEY_K },
            { "L", EventCode.KEY_L },
            { "M", EventCode.KEY_M },
            { "N", EventCode.KEY_N },
            { "O", EventCode.KEY_O },
            { "P", EventCode.KEY_P },
            { "Q", EventCode.KEY_Q },
            { "R", EventCode.KEY_R },
            { "S", EventCode.KEY_S },
            { "T", EventCode.KEY_T },
            { "U", EventCode.KEY_U },
            { "V", EventCode.KEY_V },
            { "W", EventCode.KEY_W },
            { "X", EventCode.KEY_X },
            { "Y", EventCode.KEY_Y },
            { "Z", EventCode.KEY_Z },
            { "D0", EventCode.KEY_0 },
            { "D1", EventCode.KEY_1 },
            { "D2", EventCode.KEY_2 },
            { "D3", EventCode.KEY_3 },
            { "D4", EventCode.KEY_4 },
            { "D5", EventCode.KEY_5 },
            { "D6", EventCode.KEY_6 },
            { "D7", EventCode.KEY_7 },
            { "D8", EventCode.KEY_8 },
            { "D9", EventCode.KEY_9 },
            { "F1", EventCode.KEY_F1 },
            { "F2", EventCode.KEY_F2 },
            { "F3", EventCode.KEY_F3 },
            { "F4", EventCode.KEY_F4 },
            { "F5", EventCode.KEY_F5 },
            { "F6", EventCode.KEY_F6 },
            { "F7", EventCode.KEY_F7 },
            { "F8", EventCode.KEY_F8 },
            { "F9", EventCode.KEY_F9 },
            { "F10", EventCode.KEY_F10 },
            { "F11", EventCode.KEY_F11 },
            { "F12", EventCode.KEY_F12 },
            { "Minus", EventCode.KEY_MINUS },
            { "Grave", EventCode.KEY_GRAVE },
            { "Insert", EventCode.KEY_INSERT },
            { "Home", EventCode.KEY_HOME },
            { "PageUp", EventCode.KEY_PAGEUP },
            { "PageDown", EventCode.KEY_PAGEDOWN },
            { "Delete", EventCode.KEY_DELETE },
            { "End", EventCode.KEY_END },
            { "Divide", EventCode.KEY_SLASH },
            { "Decimal", EventCode.KEY_DOT },
            { "Backspace", EventCode.KEY_BACKSPACE },
            { "Up", EventCode.KEY_UP },
            { "Down", EventCode.KEY_DOWN },
            { "Left", EventCode.KEY_LEFT },
            { "Right", EventCode.KEY_RIGHT },
            { "Tab", EventCode.KEY_TAB },
            { "Space", EventCode.KEY_SPACE },
            { "CapsLock", EventCode.KEY_CAPSLOCK },
            { "ScrollLock", EventCode.KEY_SCROLLLOCK },
            { "PrintScreen", EventCode.KEY_PRINT },
            { "NumberLock", EventCode.KEY_NUMLOCK },
            { "Enter", EventCode.KEY_ENTER },
            { "Escape", EventCode.KEY_ESC },
            { "Multiply", EventCode.KEY_NUMERIC_STAR },
            { "Add", EventCode.KEY_EQUAL },
            { "Subtract", EventCode.KEY_KPMINUS },
            { "Help", EventCode.KEY_HELP },
            { "Pause", EventCode.KEY_PAUSE },
            { "Clear", EventCode.KEY_CLEAR },
            { "KeypadEqual", EventCode.KEY_KPEQUAL },
            { "Menu", EventCode.KEY_MENU },
            { "Backslash", EventCode.KEY_BACKSLASH },
            { "Plus", EventCode.KEY_KPPLUS },
            { "Equal", EventCode.KEY_EQUAL },
            { "Semicolon", EventCode.KEY_SEMICOLON },
            { "Quote", EventCode.KEY_APOSTROPHE },
            { "Comma", EventCode.KEY_COMMA },
            { "Period", EventCode.KEY_DOT },
            { "ForwardSlash", EventCode.KEY_SLASH },
            { "Slash", EventCode.KEY_SLASH },
            { "RightBracket", EventCode.KEY_RIGHTBRACE },
            { "LeftBracket", EventCode.KEY_LEFTBRACE },
            { "ContextMenu", EventCode.KEY_CONTEXT_MENU },
            { "Keypad0", EventCode.KEY_KP0 },
            { "Keypad1", EventCode.KEY_KP1 },
            { "Keypad2", EventCode.KEY_KP2 },
            { "Keypad3", EventCode.KEY_KP3 },
            { "Keypad4", EventCode.KEY_KP4 },
            { "Keypad5", EventCode.KEY_KP5 },
            { "Keypad6", EventCode.KEY_KP6 },
            { "Keypad7", EventCode.KEY_KP7 },
            { "Keypad8", EventCode.KEY_KP8 },
            { "Keypad9", EventCode.KEY_KP9 },
            { "LeftShift", EventCode.KEY_LEFTSHIFT },
            { "RightShift", EventCode.KEY_RIGHTSHIFT },
            { "LeftControl", EventCode.KEY_LEFTCTRL },
            { "RightControl", EventCode.KEY_RIGHTCTRL },
            { "LeftAlt", EventCode.KEY_LEFTALT },
            { "RightAlt", EventCode.KEY_RIGHTALT },
            { "LeftApplication", EventCode.KEY_LEFTMETA },
            { "RightApplication", EventCode.KEY_RIGHTMETA },
            { "Shift", EventCode.KEY_LEFTSHIFT },
            { "Alt", EventCode.KEY_LEFTALT },
            { "Control", EventCode.KEY_LEFTCTRL },
            { "Application", EventCode.KEY_LEFTMETA }
        };
    }
}
