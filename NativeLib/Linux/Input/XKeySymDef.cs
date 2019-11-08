using System.ComponentModel;

namespace NativeLib.Linux.Input
{
    public enum XKeySymDef : uint
    {
        [Description("Backspace")]
        XK_BackSpace = 0xff08,
        [Description("Tab")]
        XK_Tab = 0xff09,
        [Description("Clear")]
        XK_Clear = 0xff0b,
        [Description("Return")]
        XK_Return = 0xff0d,
        [Description("Menu")]
        XK_Menu = 0xff67,
        [Description("PauseBreak")]
        XK_Pause = 0xff13,
        [Description("CapsLock")]
        XK_Caps_Lock = 0xffe5,
        [Description("Escape")]
        XK_Escape = 0xff1b,
        [Description("Space")]
        XK_space = 0x0020,
        [Description("PageUp")]
        XK_Page_Up = 0xff55,
        [Description("PageDown")]
        XK_Page_Down = 0xff56,
        [Description("End")]
        XK_End = 0xff57,
        [Description("Home")]
        XK_Home = 0xff50,
        [Description("Left")]
        XK_Left = 0xff51,
        [Description("Up")]
        XK_Up = 0xff52,
        [Description("Right")]
        XK_Right = 0xff53,
        [Description("Down")]
        XK_Down = 0xff54,
        [Description("Insert")]
        XK_Insert = 0xff63,
        [Description("Delete")]
        XK_Delete = 0xff9f,
        // Numbers are equal
        [Description("0")]
        XK_0 = 0x30,
        [Description("1")]
        XK_1 = 0x31,
        [Description("2")]
        XK_2 = 0x32,
        [Description("3")]
        XK_3 = 0x33,
        [Description("4")]
        XK_4 = 0x34,
        [Description("5")]
        XK_5 = 0x35,
        [Description("6")]
        XK_6 = 0x36,
        [Description("7")]
        XK_7 = 0x37,
        [Description("8")]
        XK_8 = 0x38,
        [Description("9")]
        XK_9 = 0x39,
        [Description("A")]
        XK_A = 0x41,
        [Description("B")]
        XK_B = 0x42,
        [Description("C")]
        XK_C = 0x43,
        [Description("D")]
        XK_D = 0x44,
        [Description("E")]
        XK_E = 0x45,
        [Description("F")]
        XK_F = 0x46,
        [Description("G")]
        XK_G = 0x47,
        [Description("H")]
        XK_H = 0x48,
        [Description("I")]
        XK_I = 0x49,
        [Description("J")]
        XK_J = 0x4A,
        [Description("K")]
        XK_K = 0x4B,
        [Description("L")]
        XK_L = 0x4C,
        [Description("M")]
        XK_M = 0x4D,
        [Description("N")]
        XK_N = 0x4E,
        [Description("O")]
        XK_O = 0x4F,
        [Description("P")]
        XK_P = 0x50,
        [Description("Q")]
        XK_Q = 0x51,
        [Description("R")]
        XK_R = 0x52,
        [Description("S")]
        XK_S = 0x53,
        [Description("T")]
        XK_T = 0x54,
        [Description("U")]
        XK_U = 0x55,
        [Description("V")]
        XK_V = 0x56,
        [Description("W")]
        XK_W = 0x57,
        [Description("X")]
        XK_X = 0x58,
        [Description("Y")]
        XK_Y = 0x59,
        [Description("Z")]
        XK_Z = 0x5A,
        [Description(";")]
        XK_OEM_102 = 0xE2,
        [Description("=")]
        XK_OEM_PLUS = 0xBB,
        [Description(",")]
        XK_OEM_COMMA = 0xBC,
        [Description("-")]
        XK_OEM_MINUS = 0xBD,
        [Description(".")]
        XK_OEM_PERIOD = 0xBE,
        [Description("/")]
        XK_OEM_2 = 0xBF,
        [Description("~")]
        XK_OEM_3 = 0xC0,
        [Description("[")]
        XK_OEM_4 = 0xDB,
        [Description("\\")]
        XK_OEM_5 = 0xDC,
        [Description("]")]
        XK_OEM_6 = 0xDD,
        [Description("\"")]
        XK_OEM_7 = 0xDE,
        // Character Control Keys
        [Description("LeftShift")]
        XK_Shift_Left = 0xffe1,
        [Description("RightShift")]
        XK_Shift_Right = 0xffe2,
        [Description("LeftControl")]
        XK_Control_Left = 0xffe3,
        [Description("RightControl")]
        XK_Control_Right = 0xffe4,
        [Description("LeftAlt")]
        XK_Meta_L = 0xffe7,
        [Description("RightAlt")]
        XK_Meta_R = 0xffe8,
        [Description("LeftSuper")]
        XK_Super_L = 0xffeb, // Tux Key
        [Description("RightSuper")]
        XK_Super_R = 0xffec, // ^
        [Description("VolumeMute")]
        XK_Volume_Mute = 0x00af,
        [Description("VolumeDown")]
        XK_Volume_Down = 0x00ae,
        [Description("VolumeUp")]
        XK_Volume_Up = 0x00ad,
        [Description("NextTrack")]
        XK_Media_Next_Track = 0x00b0,
        [Description("PreviousTrack")]
        XK_Media_Prev_Track = 0x00b1,
        [Description("Stop")]
        XK_Media_Stop = 0x00b2,
        [Description("PlayPause")]
        XK_Media_Play_Pause = 0x00b3,
        // Numpad
        [Description("Numpad0")]
        XK_KP_0 = 0xffb0,
        [Description("Numpad1")]
        XK_KP_1 = 0xffb1,
        [Description("Numpad2")]
        XK_KP_2 = 0xffb2,
        [Description("Numpad3")]
        XK_KP_3 = 0xffb3,
        [Description("Numpad4")]
        XK_KP_4 = 0xffb4,
        [Description("Numpad5")]
        XK_KP_5 = 0xffb5,
        [Description("Numpad6")]
        XK_KP_6 = 0xffb6,
        [Description("Numpad7")]
        XK_KP_7 = 0xffb7,
        [Description("Numpad8")]
        XK_KP_8 = 0xffb8,
        [Description("Numpad9")]
        XK_KP_9 = 0xffb9,
        [Description("NumpadMultiply")]
        XK_Multiply = 0xffaa,
        [Description("NumpadAdd")]
        XK_Add = 0xffab,
        [Description("NumpadSeparator")]
        XK_Separator = 0xffac,
        [Description("NumpadSubtract")]
        XK_Subtract = 0xffad,
        [Description("NumpadDecimal")]
        XK_Decimal = 0xffae,
        [Description("NumpadDivide")]
        XK_Divide = 0xffaf,
        // Function Keys
        [Description("F1")]
        XK_F1 = 0xffbe,
        [Description("F2")]
        XK_F2 = 0xffbf,
        [Description("F3")]
        XK_F3 = 0xffc0,
        [Description("F4")]
        XK_F4 = 0xffc1,
        [Description("F5")]
        XK_F5 = 0xffc2,
        [Description("F6")]
        XK_F6 = 0xffc3,
        [Description("F7")]
        XK_F7 = 0xffc4,
        [Description("F8")]
        XK_F8 = 0xffc5,
        [Description("F9")]
        XK_F9 = 0xffc6,
        [Description("F10")]
        XK_F10 = 0xffc7,
        [Description("F11")]
        XK_F11 = 0xffc8,
        [Description("F12")]
        XK_F12 = 0xffc9,
        [Description("F13")]
        XK_F13 = 0xffca,
        [Description("F14")]
        XK_F14 = 0xffcb,
        [Description("F15")]
        XK_F15 = 0xffcc,
        [Description("F16")]
        XK_F16 = 0xffcd,
        [Description("F17")]
        XK_F17 = 0xffce,
        [Description("F18")]
        XK_F18 = 0xffcf,
        [Description("F19")]
        XK_F19 = 0xffd0,
        [Description("F20")]
        XK_F20 = 0xffd1,
        [Description("F21")]
        XK_F21 = 0xffd2,
        [Description("F22")]
        XK_F22 = 0xffd3,
        [Description("F23")]
        XK_F23 = 0xffd4,
        [Description("F24")]
        XK_F24 = 0xffd5
    }
}