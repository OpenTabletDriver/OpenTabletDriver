using System.Collections.Generic;
using OpenTabletDriver.Native.MacOS.Input;
using OpenTabletDriver.Platform.Keyboard;

namespace OpenTabletDriver.Daemon.Interop.Input.Keyboard
{
    public class MacOSKeysProvider : IKeyMapper
    {
        private static readonly Dictionary<BindableKey, CGKeyCode> _platformBindings = new()
        {
            // [BindableKey.Cancel]
            [BindableKey.Backspace] = CGKeyCode.kVK_Delete,
            [BindableKey.Tab] = CGKeyCode.kVK_Tab,
            // [BindableKey.Clear]
            [BindableKey.Enter] = CGKeyCode.kVK_Return,
            [BindableKey.Shift] = CGKeyCode.kVK_Shift,
            [BindableKey.Control] = CGKeyCode.kVK_Control,
            [BindableKey.Alt] = CGKeyCode.kVK_Option,
            // [BindableKey.Pause]
            [BindableKey.CapsLock] = CGKeyCode.kVK_CapsLock,
            [BindableKey.Escape] = CGKeyCode.kVK_Escape,
            [BindableKey.Space] = CGKeyCode.kVK_Space,
            [BindableKey.PageUp] = CGKeyCode.kVK_PageUp,
            [BindableKey.PageDown] = CGKeyCode.kVK_PageDown,
            [BindableKey.End] = CGKeyCode.kVK_End,
            [BindableKey.Home] = CGKeyCode.kVK_Home,
            [BindableKey.Left] = CGKeyCode.kVK_LeftArrow,
            [BindableKey.Up] = CGKeyCode.kVK_UpArrow,
            [BindableKey.Right] = CGKeyCode.kVK_RightArrow,
            [BindableKey.Down] = CGKeyCode.kVK_DownArrow,
            // [BindableKey.Select]
            // [BindableKey.Print]
            // [BindableKey.Execute]
            // [BindableKey.PrintScreen]
            // [BindableKey.Insert]
            [BindableKey.Delete] = CGKeyCode.kVK_ForwardDelete,
            // [BindableKey.Help]
            [BindableKey.D0] = CGKeyCode.kVK_ANSI_0,
            [BindableKey.D1] = CGKeyCode.kVK_ANSI_1,
            [BindableKey.D2] = CGKeyCode.kVK_ANSI_2,
            [BindableKey.D3] = CGKeyCode.kVK_ANSI_3,
            [BindableKey.D4] = CGKeyCode.kVK_ANSI_4,
            [BindableKey.D5] = CGKeyCode.kVK_ANSI_5,
            [BindableKey.D6] = CGKeyCode.kVK_ANSI_6,
            [BindableKey.D7] = CGKeyCode.kVK_ANSI_7,
            [BindableKey.D8] = CGKeyCode.kVK_ANSI_8,
            [BindableKey.D9] = CGKeyCode.kVK_ANSI_9,
            [BindableKey.A] = CGKeyCode.kVK_ANSI_A,
            [BindableKey.B] = CGKeyCode.kVK_ANSI_B,
            [BindableKey.C] = CGKeyCode.kVK_ANSI_C,
            [BindableKey.D] = CGKeyCode.kVK_ANSI_D,
            [BindableKey.E] = CGKeyCode.kVK_ANSI_E,
            [BindableKey.F] = CGKeyCode.kVK_ANSI_F,
            [BindableKey.G] = CGKeyCode.kVK_ANSI_G,
            [BindableKey.H] = CGKeyCode.kVK_ANSI_H,
            [BindableKey.I] = CGKeyCode.kVK_ANSI_I,
            [BindableKey.J] = CGKeyCode.kVK_ANSI_J,
            [BindableKey.K] = CGKeyCode.kVK_ANSI_K,
            [BindableKey.L] = CGKeyCode.kVK_ANSI_L,
            [BindableKey.M] = CGKeyCode.kVK_ANSI_M,
            [BindableKey.N] = CGKeyCode.kVK_ANSI_N,
            [BindableKey.O] = CGKeyCode.kVK_ANSI_O,
            [BindableKey.P] = CGKeyCode.kVK_ANSI_P,
            [BindableKey.Q] = CGKeyCode.kVK_ANSI_Q,
            [BindableKey.R] = CGKeyCode.kVK_ANSI_R,
            [BindableKey.S] = CGKeyCode.kVK_ANSI_S,
            [BindableKey.T] = CGKeyCode.kVK_ANSI_T,
            [BindableKey.U] = CGKeyCode.kVK_ANSI_U,
            [BindableKey.V] = CGKeyCode.kVK_ANSI_V,
            [BindableKey.W] = CGKeyCode.kVK_ANSI_W,
            [BindableKey.X] = CGKeyCode.kVK_ANSI_X,
            [BindableKey.Y] = CGKeyCode.kVK_ANSI_Y,
            [BindableKey.Z] = CGKeyCode.kVK_ANSI_Z,
            [BindableKey.LeftWindows] = CGKeyCode.kVK_Command,
            [BindableKey.RightWindows] = CGKeyCode.kVK_RightCommand,
            [BindableKey.Applications] = CGKeyCode.kVK_Command,
            // [BindableKey.Sleep]
            [BindableKey.Numpad0] = CGKeyCode.kVK_ANSI_Keypad0,
            [BindableKey.Numpad1] = CGKeyCode.kVK_ANSI_Keypad1,
            [BindableKey.Numpad2] = CGKeyCode.kVK_ANSI_Keypad2,
            [BindableKey.Numpad3] = CGKeyCode.kVK_ANSI_Keypad3,
            [BindableKey.Numpad4] = CGKeyCode.kVK_ANSI_Keypad4,
            [BindableKey.Numpad5] = CGKeyCode.kVK_ANSI_Keypad5,
            [BindableKey.Numpad6] = CGKeyCode.kVK_ANSI_Keypad6,
            [BindableKey.Numpad7] = CGKeyCode.kVK_ANSI_Keypad7,
            [BindableKey.Numpad8] = CGKeyCode.kVK_ANSI_Keypad8,
            [BindableKey.Numpad9] = CGKeyCode.kVK_ANSI_Keypad9,
            [BindableKey.Multiply] = CGKeyCode.kVK_ANSI_KeypadMultiply,
            [BindableKey.Add] = CGKeyCode.kVK_ANSI_KeypadPlus,
            [BindableKey.Separator] = CGKeyCode.kVK_ANSI_Comma,
            [BindableKey.Subtract] = CGKeyCode.kVK_ANSI_KeypadMinus,
            [BindableKey.Decimal] = CGKeyCode.kVK_ANSI_KeypadDecimal,
            [BindableKey.Divide] = CGKeyCode.kVK_ANSI_KeypadDivide,
            [BindableKey.F1] = CGKeyCode.kVK_F1,
            [BindableKey.F2] = CGKeyCode.kVK_F2,
            [BindableKey.F3] = CGKeyCode.kVK_F3,
            [BindableKey.F4] = CGKeyCode.kVK_F4,
            [BindableKey.F5] = CGKeyCode.kVK_F5,
            [BindableKey.F6] = CGKeyCode.kVK_F6,
            [BindableKey.F7] = CGKeyCode.kVK_F7,
            [BindableKey.F8] = CGKeyCode.kVK_F8,
            [BindableKey.F9] = CGKeyCode.kVK_F9,
            [BindableKey.F10] = CGKeyCode.kVK_F10,
            [BindableKey.F11] = CGKeyCode.kVK_F11,
            [BindableKey.F12] = CGKeyCode.kVK_F12,
            [BindableKey.F13] = CGKeyCode.kVK_F13,
            [BindableKey.F14] = CGKeyCode.kVK_F14,
            [BindableKey.F15] = CGKeyCode.kVK_F15,
            [BindableKey.F16] = CGKeyCode.kVK_F16,
            [BindableKey.F17] = CGKeyCode.kVK_F17,
            [BindableKey.F18] = CGKeyCode.kVK_F18,
            [BindableKey.F19] = CGKeyCode.kVK_F19,
            [BindableKey.F20] = CGKeyCode.kVK_F20,
            // [BindableKey.F21]
            // [BindableKey.F22]
            // [BindableKey.F23]
            // [BindableKey.F24]
            // [BindableKey.NumLock]
            // [BindableKey.Scroll]
            [BindableKey.LeftShift] = CGKeyCode.kVK_Shift,
            [BindableKey.RightShift] = CGKeyCode.kVK_RightShift,
            [BindableKey.LeftControl] = CGKeyCode.kVK_Control,
            [BindableKey.RightControl] = CGKeyCode.kVK_RightControl,
            [BindableKey.LeftAlt] = CGKeyCode.kVK_Option,
            [BindableKey.RightAlt] = CGKeyCode.kVK_RightOption,
            // [BindableKey.BrowserBack]
            // [BindableKey.BrowserForward]
            // [BindableKey.BrowserRefresh]
            // [BindableKey.BrowserStop]
            // [BindableKey.BrowserSearch]
            // [BindableKey.BrowserFavorites]
            // [BindableKey.BrowserHome]
            [BindableKey.VolumeMute] = CGKeyCode.kVK_Mute,
            [BindableKey.VolumeDown] = CGKeyCode.kVK_VolumeDown,
            [BindableKey.VolumeUp] = CGKeyCode.kVK_VolumeUp,
            // [BindableKey.MediaNextTrack]
            // [BindableKey.MediaPreviousTrack]
            // [BindableKey.MediaStop]
            // [BindableKey.MediaPlayPause]
            // [BindableKey.LaunchMail]
            // [BindableKey.SelectMedia]
            // [BindableKey.LaunchApplication1]
            // [BindableKey.LaunchApplication2]
            [BindableKey.Oem1] = CGKeyCode.kVK_ANSI_Semicolon,
            [BindableKey.OemPlus] = CGKeyCode.kVK_ANSI_KeypadPlus,
            [BindableKey.OemComma] = CGKeyCode.kVK_ANSI_Comma,
            [BindableKey.OemMinus] = CGKeyCode.kVK_ANSI_Minus,
            [BindableKey.OemPeriod] = CGKeyCode.kVK_ANSI_Period,
            [BindableKey.OemQuestion] = CGKeyCode.kVK_ANSI_Slash,
            [BindableKey.OemTilde] = CGKeyCode.kVK_ANSI_Grave,
            [BindableKey.OemOpenBracket] = CGKeyCode.kVK_ANSI_LeftBracket,
            [BindableKey.OemPipe] = CGKeyCode.kVK_ANSI_Backslash,
            [BindableKey.OemCloseBracket] = CGKeyCode.kVK_ANSI_RightBracket,
            [BindableKey.OemQuotes] = CGKeyCode.kVK_ANSI_Quote,
            // [BindableKey.Oem8]
            // [BindableKey.Oem102]
        };

        public object this[BindableKey key] => _platformBindings[key];
        public IEnumerable<BindableKey> GetBindableKeys() => _platformBindings.Keys;
    }
}
