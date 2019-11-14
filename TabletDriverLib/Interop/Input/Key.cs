using System.ComponentModel;

namespace TabletDriverLib.Interop.Input
{
    // All of these are based off of the Windows VirtualKey keycodes.
    public enum Key
    {
        // Generic Keys
        [Description("Backspace")]
        Backspace = 0x08,
        [Description("Tab")]
        Tab = 0x09,
        [Description("Clear")]
        Clear = 0x0C,
        [Description("Return")]
        Return = 0x0D,
        [Description("PauseBreak")]
        PauseBreak = 0x13,
        [Description("CapsLock")]
        CapsLock = 0x14,
        [Description("Escape")]
        Escape = 0x1B,
        [Description("Space")]
        Space = 0x20,
        [Description("PageUp")]
        PageUp = 0x21,
        [Description("PageDown")]
        PageDown = 0x22,
        [Description("End")]
        End = 0x23,
        [Description("Home")]
        Home = 0x24,
        [Description("Left")]
        Left = 0x25,
        [Description("Up")]
        Up = 0x26,
        [Description("Right")]
        Right = 0x27,
        [Description("Down")]
        Down = 0x28,
        [Description("Insert")]
        Insert = 0x2D,
        [Description("Delete")]
        Delete = 0x2E,
        // Letter and Number Keys
        [Description("0")]
        D0 = 0x30,
        [Description("1")]
        D1 = 0x31,
        [Description("2")]
        D2 = 0x32,
        [Description("3")]
        D3 = 0x33,
        [Description("4")]
        D4 = 0x34,
        [Description("5")]
        D5 = 0x35,
        [Description("6")]
        D6 = 0x36,
        [Description("7")]
        D7 = 0x37,
        [Description("8")]
        D8 = 0x38,
        [Description("9")]
        D9 = 0x39,
        [Description("A")]
        A = 0x41,
        [Description("B")]
        B = 0x42,
        [Description("C")]
        C = 0x43,
        [Description("D")]
        D = 0x44,
        [Description("E")]
        E = 0x45,
        [Description("F")]
        F = 0x46,
        [Description("G")]
        G = 0x47,
        [Description("H")]
        H = 0x48,
        [Description("I")]
        I = 0x49,
        [Description("J")]
        J = 0x4A,
        [Description("K")]
        K = 0x4B,
        [Description("L")]
        L = 0x4C,
        [Description("M")]
        M = 0x4D,
        [Description("N")]
        N = 0x4E,
        [Description("O")]
        O = 0x4F,
        [Description("P")]
        P = 0x50,
        [Description("Q")]
        Q = 0x51,
        [Description("R")]
        R = 0x52,
        [Description("S")]
        S = 0x53,
        [Description("T")]
        T = 0x54,
        [Description("U")]
        U = 0x55,
        [Description("V")]
        V = 0x56,
        [Description("W")]
        W = 0x57,
        [Description("X")]
        X = 0x58,
        [Description("Y")]
        Y = 0x59,
        [Description("Z")]
        Z = 0x5A,
        [Description(";")]
        Semicolon = 0xBA,
        [Description("=")]
        PlusEqual = 0xBB,
        [Description(",")]
        Comma = 0xBC,
        [Description("-")]
        Minus = 0xBD,
        [Description(".")]
        Period = 0xBE,
        [Description("/")]
        ForwardSlash = 0xBF,
        [Description("~")]
        Grave = 0xC0,
        [Description("[")]
        LeftBracket = 0xDB,
        [Description("\\")]
        Backslash = 0xDC,
        [Description("]")]
        RightBracket = 0xDD,
        [Description("\"")]
        Quote = 0xDE,
        // Character Control Keys
        [Description("LeftShift")]
        LeftShift = 0xA0,
        [Description("RightShift")]
        RightShift = 0xA1,
        [Description("LeftControl")]
        LeftControl = 0xA2,
        [Description("RightControl")]
        RightControl = 0xA3,
        [Description("LeftAlt")]
        LeftMenu = 0xA4,
        [Description("RightAlt")]
        RightMenu = 0xA5,
        [Description("LeftSuper")]
        LeftSuper = 0x5B, // Windows Key, Tux Key, Apple Key
        [Description("RightSuper")]
        RightSuper = 0x5C, // ^
        // Media keys
        [Description("VolumeMute")]
        VolumeMute = 0xAD,
        [Description("VolumeDown")]
        VolumeDown = 0xAE,
        [Description("VolumeUp")]
        VolumeUp = 0xAF,
        [Description("NextTrack")]
        NextTrack = 0xB0,
        [Description("PreviousTrack")]
        PreviousTrack = 0xB1,
        [Description("Stop")]
        Stop = 0xB2,
        [Description("PlayPause")]
        PlayPause = 0xB3,
        // Numpad
        [Description("Numpad0")]
        Numpad0 = 0x60,
        [Description("Numpad1")]
        Numpad1 = 0x61,
        [Description("Numpad2")]
        Numpad2 = 0x62,
        [Description("Numpad3")]
        Numpad3 = 0x63,
        [Description("Numpad4")]
        Numpad4 = 0x64,
        [Description("Numpad5")]
        Numpad5 = 0x65,
        [Description("Numpad6")]
        Numpad6 = 0x66,
        [Description("Numpad7")]
        Numpad7 = 0x67,
        [Description("Numpad8")]
        Numpad8 = 0x68,
        [Description("Numpad9")]
        Numpad9 = 0x69,
        [Description("NumpadMultiply")]
        NumpadMultiply = 0x6A,
        [Description("NumpadAdd")]
        NumpadAdd = 0x6B,
        [Description("NumpadSeparator")]
        NumpadSeparator = 0x6C,
        [Description("NumpadSubtract")]
        NumpadSubtract = 0x6D,
        [Description("NumpadDecimal")]
        NumpadDecimal = 0x6E,
        [Description("NumpadDivide")]
        NumpadDivide = 0x6F,
        // Function Keys
        [Description("F1")]
        F1 = 0x70,
        [Description("F2")]
        F2 = 0x71,
        [Description("F3")]
        F3 = 0x72,
        [Description("F4")]
        F4 = 0x73,
        [Description("F5")]
        F5 = 0x74,
        [Description("F6")]
        F6 = 0x75,
        [Description("F7")]
        F7 = 0x76,
        [Description("F8")]
        F8 = 0x77,
        [Description("F9")]
        F9 = 0x78,
        [Description("F10")]
        F10 = 0x79,
        [Description("F11")]
        F11 = 0x7A,
        [Description("F12")]
        F12 = 0x7B,
        [Description("F13")]
        F13 = 0x7C,
        [Description("F14")]
        F14 = 0x7D,
        [Description("F15")]
        F15 = 0x7E,
        [Description("F16")]
        F16 = 0x7F,
        [Description("F17")]
        F17 = 0x80,
        [Description("F18")]
        F18 = 0x81,
        [Description("F19")]
        F19 = 0x82,
        [Description("F20")]
        F20 = 0x83,
        [Description("F21")]
        F21 = 0x84,
        [Description("F22")]
        F22 = 0x85,
        [Description("F23")]
        F23 = 0x86,
        [Description("F24")]
        F24 = 0x87,
    }
}