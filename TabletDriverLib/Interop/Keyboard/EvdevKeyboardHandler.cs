using System;
using System.Collections.Generic;
using System.Linq;
using NativeLib.Linux;
using NativeLib.Linux.Evdev;
using TabletDriverPlugin;

namespace TabletDriverLib.Interop.Keyboard
{
    public class EvdevKeyboardHandler : IKeyboardHandler, IDisposable
    {
        public EvdevKeyboardHandler()
        {
            Device = new EvdevDevice("OpenTabletDriver Virtual Keyboard");

            Device.EnableTypeCodes(EventType.EV_KEY, XKeysymToEventCode.Values.Distinct().ToArray());

            var result = Device.Initialize();
            switch (result)
            {
                case ERRNO.NONE:
                    Log.Debug($"Successfully initialized virtual keyboard. (code {result})");
                    break;
                default:
                    Log.Write("Evdev", $"Failed to initialize virtual keyboard. (error code {result})", true);
                    break;
            }
        }

        private EvdevDevice Device { set; get; }

        private void KeyPress(string key, bool isPress)
        {
            var keyEventCode = XKeysymToEventCode[key];

            Device.Write(EventType.EV_KEY, keyEventCode, isPress ? 1 : 0);
            Device.Sync();
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
            Device?.Dispose();
        }

        public static readonly Dictionary<string, EventCode> XKeysymToEventCode = new Dictionary<string, EventCode>
        {
            { "BackSpace", EventCode.KEY_BACKSPACE },
            { "Tab", EventCode.KEY_TAB },
            { "Linefeed", EventCode.KEY_LINEFEED },
            { "Clear", EventCode.KEY_CLEAR },
            { "Return", EventCode.KEY_ENTER },
            { "Pause", EventCode.KEY_PAUSE },
            { "Scroll_Lock", EventCode.KEY_SCROLLLOCK },
            { "Escape", EventCode.KEY_ESC },
            { "Delete", EventCode.KEY_DELETE },
            { "Home", EventCode.KEY_HOME },
            { "Left", EventCode.KEY_LEFT },
            { "Up", EventCode.KEY_UP },
            { "Right", EventCode.KEY_RIGHT },
            { "Down", EventCode.KEY_DOWN },
            { "Prior", EventCode.KEY_PREVIOUS },
            { "Page_Up", EventCode.KEY_PAGEUP },
            { "Next", EventCode.KEY_NEXT },
            { "Page_Down", EventCode.KEY_PAGEDOWN },
            { "End", EventCode.KEY_END },
            { "Begin", EventCode.KEY_HOME },
            { "Select", EventCode.KEY_SELECT },
            { "Print", EventCode.KEY_PRINT },
            { "Insert", EventCode.KEY_INSERT },
            { "Menu", EventCode.KEY_MENU },
            { "Cancel", EventCode.KEY_CANCEL },
            { "Help", EventCode.KEY_HELP },
            { "Break", EventCode.KEY_BREAK },
            { "Num_Lock", EventCode.KEY_NUMLOCK },
            { "KP_Space", EventCode.KEY_SPACE },
            { "KP_Tab", EventCode.KEY_TAB },
            { "KP_Enter", EventCode.KEY_KPENTER },
            { "KP_Home", EventCode.KEY_HOME },
            { "KP_Left", EventCode.KEY_LEFT },
            { "KP_Up", EventCode.KEY_UP },
            { "KP_Right", EventCode.KEY_RIGHT },
            { "KP_Down", EventCode.KEY_DOWN },
            { "KP_Prior", EventCode.KEY_PREVIOUS },
            { "KP_Page_Up", EventCode.KEY_PAGEUP },
            { "KP_Next", EventCode.KEY_NEXT },
            { "KP_Page_Down", EventCode.KEY_PAGEDOWN },
            { "KP_End", EventCode.KEY_END },
            { "KP_Begin", EventCode.KEY_HOME },
            { "KP_Insert", EventCode.KEY_KP0 },
            { "KP_Delete", EventCode.KEY_KPDOT },
            { "KP_Multiply", EventCode.KEY_KPASTERISK },
            { "KP_Add", EventCode.KEY_KPPLUS },
            { "KP_Separator", EventCode.KEY_MINUS },
            { "KP_Subtract", EventCode.KEY_KPMINUS },
            { "KP_Decimal", EventCode.KEY_KPDOT },
            { "KP_Divide", EventCode.KEY_KPSLASH },
            { "KP_0", EventCode.KEY_KP0 },
            { "KP_1", EventCode.KEY_KP1 },
            { "KP_2", EventCode.KEY_KP2 },
            { "KP_3", EventCode.KEY_KP3 },
            { "KP_4", EventCode.KEY_KP4 },
            { "KP_5", EventCode.KEY_KP5 },
            { "KP_6", EventCode.KEY_KP6 },
            { "KP_7", EventCode.KEY_KP7 },
            { "KP_8", EventCode.KEY_KP8 },
            { "KP_9", EventCode.KEY_KP9 },
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
            { "F13", EventCode.KEY_F13 },
            { "F14", EventCode.KEY_F14 },
            { "F15", EventCode.KEY_F15 },
            { "F16", EventCode.KEY_F16 },
            { "F17", EventCode.KEY_F17 },
            { "F18", EventCode.KEY_F18 },
            { "F19", EventCode.KEY_F19 },
            { "F20", EventCode.KEY_F20 },
            { "F21", EventCode.KEY_F21 },
            { "F22", EventCode.KEY_F22 },
            { "F23", EventCode.KEY_F23 },
            { "F24", EventCode.KEY_F24 },
            { "Shift_L", EventCode.KEY_LEFTSHIFT },
            { "Shift_R", EventCode.KEY_RIGHTSHIFT },
            { "Control_L", EventCode.KEY_LEFTCTRL },
            { "Control_R", EventCode.KEY_RIGHTCTRL },
            { "Caps_Lock", EventCode.KEY_CAPSLOCK },
            { "Super_L", EventCode.KEY_LEFTMETA },
            { "Super_R", EventCode.KEY_RIGHTMETA },
            { "space", EventCode.KEY_SPACE },
            { "apostrophe", EventCode.KEY_APOSTROPHE },
            { "quoteright", EventCode.KEY_APOSTROPHE },
            { "plus", EventCode.KEY_EQUAL },
            { "comma", EventCode.KEY_COMMA },
            { "minus", EventCode.KEY_MINUS },
            { "period", EventCode.KEY_DOT },
            { "slash", EventCode.KEY_SLASH },
            { "0", EventCode.KEY_0 },
            { "1", EventCode.KEY_1 },
            { "2", EventCode.KEY_2 },
            { "3", EventCode.KEY_3 },
            { "4", EventCode.KEY_4 },
            { "5", EventCode.KEY_5 },
            { "6", EventCode.KEY_6 },
            { "7", EventCode.KEY_7 },
            { "8", EventCode.KEY_8 },
            { "9", EventCode.KEY_9 },
            { "colon", EventCode.KEY_SEMICOLON },
            { "semicolon", EventCode.KEY_SEMICOLON },
            { "question", EventCode.KEY_QUESTION },
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
            { "bracketleft", EventCode.KEY_LEFTBRACE },
            { "backslash", EventCode.KEY_BACKSLASH },
            { "bracketright", EventCode.KEY_RIGHTBRACE },
            { "underscore", EventCode.KEY_MINUS },
            { "grave", EventCode.KEY_GRAVE },
            { "quoteleft", EventCode.KEY_APOSTROPHE },
            { "a", EventCode.KEY_A },
            { "b", EventCode.KEY_B },
            { "c", EventCode.KEY_C },
            { "d", EventCode.KEY_D },
            { "e", EventCode.KEY_E },
            { "f", EventCode.KEY_F },
            { "g", EventCode.KEY_G },
            { "h", EventCode.KEY_H },
            { "i", EventCode.KEY_I },
            { "j", EventCode.KEY_J },
            { "k", EventCode.KEY_K },
            { "l", EventCode.KEY_L },
            { "m", EventCode.KEY_M },
            { "n", EventCode.KEY_N },
            { "o", EventCode.KEY_O },
            { "p", EventCode.KEY_P },
            { "q", EventCode.KEY_Q },
            { "r", EventCode.KEY_R },
            { "s", EventCode.KEY_S },
            { "t", EventCode.KEY_T },
            { "u", EventCode.KEY_U },
            { "v", EventCode.KEY_V },
            { "w", EventCode.KEY_W },
            { "x", EventCode.KEY_X },
            { "y", EventCode.KEY_Y },
            { "z", EventCode.KEY_Z },
            { "braceleft", EventCode.KEY_LEFTBRACE },
            { "bar", EventCode.KEY_MINUS },
            { "braceright", EventCode.KEY_RIGHTBRACE },
            { "asciitilde", EventCode.KEY_GRAVE },
        };
    }
}