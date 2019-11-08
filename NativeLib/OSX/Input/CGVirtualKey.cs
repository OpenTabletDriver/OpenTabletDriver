using System.ComponentModel;

namespace NativeLib.OSX.Input
{
    public enum CGVirtualKey
    {
        [Description("Backspace")]
        kVK_Delete = 0x33,
        [Description("Tab")]
        kVK_Tab = 0x30,
        [Description("Clear")]
        kVK_ANSI_KeypadClear = 0x47,
        [Description("Return")]
        // [Description("PauseBreak")]
        kVK_Return = 0x24,
        [Description("CapsLock")]
        kVK_CapsLock = 0x39,
        [Description("Escape")]
        kVK_Escape = 0x35,
        [Description("Space")]
        kVK_Space = 0x31,
        [Description("PageUp")]
        kVK_PageUp = 0x74,
        [Description("PageDown")]
        kVK_PageDown = 0x79,
        [Description("End")]
        kVK_End = 0x77,
        [Description("Home")]
        kVK_Home = 0x73,
        [Description("Left")]
        kVK_LeftArrow = 0x7B,
        [Description("Up")]
        kVK_UpArrow = 0x7E,
        [Description("Right")]
        kVK_RightArrow = 0x7C,
        [Description("Down")]
        kVK_DownArrow = 0x7D,
        [Description("Delete")]
        kVK_ForwardDelete = 0x75,
        [Description("0")]
        kVK_ANSI_0 = 0x1D,
        [Description("1")]
        kVK_ANSI_1 = 0x12,
        [Description("2")]
        kVK_ANSI_2 = 0x13,
        [Description("3")]
        kVK_ANSI_3 = 0x14,
        [Description("4")]
        kVK_ANSI_4 = 0x15,
        [Description("5")]
        kVK_ANSI_5 = 0x17,
        [Description("6")]
        kVK_ANSI_6 = 0x16,
        [Description("7")]
        kVK_ANSI_7 = 0x1A,
        [Description("8")]
        kVK_ANSI_8 = 0x1C,
        [Description("9")]
        kVK_ANSI_9 = 0x19,
        [Description("A")]
        kVK_ANSI_A = 0x00,
        [Description("B")]
        kVK_ANSI_B = 0x0B,
        [Description("C")]
        kVK_ANSI_C = 0x08,
        [Description("D")]
        kVK_ANSI_D = 0x02,
        [Description("E")]
        kVK_ANSI_E = 0x0E,
        [Description("F")]
        kVK_ANSI_F = 0x03,
        [Description("G")]
        kVK_ANSI_G = 0x05,
        [Description("H")]
        kVK_ANSI_H = 0x04,
        [Description("I")]
        kVK_ANSI_I = 0x22,
        [Description("J")]
        kVK_ANSI_J = 0x26,
        [Description("K")]
        kVK_ANSI_K = 0x28,
        [Description("L")]
        kVK_ANSI_L = 0x25,
        [Description("M")]
        kVK_ANSI_M = 0x2E,
        [Description("N")]
        kVK_ANSI_N = 0x2D,
        [Description("O")]
        kVK_ANSI_O = 0x1F,
        [Description("P")]
        kVK_ANSI_P = 0x23,
        [Description("Q")]
        kVK_ANSI_Q = 0x0C,
        [Description("R")]
        kVK_ANSI_R = 0x0F,
        [Description("S")]
        kVK_ANSI_S = 0x01,
        [Description("T")]
        kVK_ANSI_T = 0x11,
        [Description("U")]
        kVK_ANSI_U = 0x20,
        [Description("V")]
        kVK_ANSI_V = 0x09,
        [Description("W")]
        kVK_ANSI_W = 0x0D,
        [Description("X")]
        kVK_ANSI_X = 0x07,
        [Description("Y")]
        kVK_ANSI_Y = 0x10,
        [Description("Z")]
        kVK_ANSI_Z = 0x06,
        [Description(";")]
        kVK_ANSI_Semicolon = 0x29,
        [Description("=")]
        kVK_ANSI_Equal = 0x18,
        [Description(",")]
        kVK_ANSI_Comma = 0x2B,
        [Description("-")]
        kVK_ANSI_Minus = 0x1B,
        [Description(".")]
        kVK_ANSI_Period = 0x2F,
        [Description("/")]
        kVK_ANSI_Slash = 0x2C,
        [Description("~")]
        kVK_ANSI_Grave = 0x32,
        [Description("[")]
        kVK_ANSI_LeftBracket = 0x21,
        [Description("\\")]
        kVK_ANSI_Backslash = 0x2A,
        [Description("]")]
        kVK_ANSI_RightBracket = 0x1E,
        [Description("\"")]
        kVK_ANSI_Quote = 0x27,
        [Description("LeftShift")]
        kVK_Shift = 0x38,
        [Description("RightShift")]
        kVK_RightShift = 0x3C,
        [Description("LeftControl")]
        kVK_Control = 0x3B,
        [Description("RightControl")]
        kVK_RightControl = 0x3E,
        [Description("LeftAlt")]
        kVK_Option = 0x3A,
        [Description("RightAlt")]
        kVK_RightOption = 0x3D,
        // [Description("LeftSuper")]
        // [Description("RightSuper")]
        [Description("VolumeMute")]
        kVK_Mute = 0x4A,
        [Description("VolumeDown")]
        kVK_VolumeDown = 0x49,
        [Description("VolumeUp")]
        // [Description("NextTrack")]
        // [Description("PreviousTrack")]
        // [Description("Stop")]
        // [Description("PlayPause")]
        kVK_VolumeUp = 0x48,
        [Description("Numpad0")]
        kVK_ANSI_Keypad0 = 0x52,
        [Description("Numpad1")]
        kVK_ANSI_Keypad1 = 0x53,
        [Description("Numpad2")]
        kVK_ANSI_Keypad2 = 0x54,
        [Description("Numpad3")]
        kVK_ANSI_Keypad3 = 0x55,
        [Description("Numpad4")]
        kVK_ANSI_Keypad4 = 0x56,
        [Description("Numpad5")]
        kVK_ANSI_Keypad5 = 0x57,
        [Description("Numpad6")]
        kVK_ANSI_Keypad6 = 0x58,
        [Description("Numpad7")]
        kVK_ANSI_Keypad7 = 0x59,
        [Description("Numpad8")]
        kVK_ANSI_Keypad8 = 0x5B,
        [Description("Numpad9")]
        kVK_ANSI_Keypad9 = 0x5C,
        [Description("NumpadMultiply")]
        kVK_ANSI_KeypadMultiply = 0x43,
        [Description("NumpadAdd")]
        kVK_ANSI_KeypadPlus = 0x45,
        [Description("NumpadMinus")]
        kVK_ANSI_KeypadMinus = 0x4E,
        [Description("NumpadDecimal")]
        kVK_ANSI_KeypadDecimal = 0x41,
        [Description("NumpadDivide")]
        kVK_ANSI_KeypadDivide = 0x4B,
        [Description("F1")]
        kVK_F1 = 0x7A,
        [Description("F2")]
        kVK_F2 = 0x78,
        [Description("F3")]
        kVK_F3 = 0x63,
        [Description("F4")]
        kVK_F4 = 0x76,
        [Description("F5")]
        kVK_F5 = 0x60,
        [Description("F6")]
        kVK_F6 = 0x61,
        [Description("F7")]
        kVK_F7 = 0x62,
        [Description("F8")]
        kVK_F8 = 0x64,
        [Description("F9")]
        kVK_F9 = 0x65,
        [Description("F10")]
        kVK_F10 = 0x6D,
        [Description("F11")]
        kVK_F11 = 0x67,
        [Description("F12")]
        kVK_F12 = 0x6F,
        [Description("F13")]
        kVK_F13 = 0x69,
        [Description("F14")]
        kVK_F14 = 0x6B,
        [Description("F15")]
        kVK_F15 = 0x71,
        [Description("F16")]
        kVK_F16 = 0x6A,
        [Description("F17")]
        kVK_F17 = 0x40,
        [Description("F18")]
        kVK_F18 = 0x4F,
        [Description("F19")]
        kVK_F19 = 0x50,
        [Description("F20")]
        kVK_F20 = 0x5A,
        // [Description("F21")]
        // [Description("F22")]
        // [Description("F23")]
        // [Description("F24")]
        // The rest are left over, missing an equal representative.
        kVK_ANSI_KeypadEnter = 0x4C,
        kVK_ANSI_KeypadEquals = 0x51,
        kVK_Command = 0x37,
        kVK_Function = 0x3F,
        kVK_Help = 0x72,
    }
}