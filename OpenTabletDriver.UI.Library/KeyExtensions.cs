using Avalonia.Input;
using OpenTabletDriver.Platform.Keyboard;

namespace OpenTabletDriver.UI;

public static class KeyExtensions
{
    public static BindableKey ToBindableKey(this Key key)
    {
        return key switch
        {
            Key.None => BindableKey.None,
            Key.Cancel => BindableKey.Cancel,
            Key.Back => BindableKey.Backspace,
            Key.Tab => BindableKey.Tab,
            // Key.LineFeed
            Key.Clear => BindableKey.Clear,
            // Key.Return => BindableKey.Return,
            Key.Enter => BindableKey.Enter,
            Key.Pause => BindableKey.Pause,
            Key.CapsLock => BindableKey.CapsLock,
            // Key.Capital => BindableKey.CapsLock,
            // Key.HangulMode
            // Key.KanaMode
            // Key.JunjaMode
            // Key.FinalMode
            // Key.KanjiMode
            // Key.HanjaMode
            Key.Escape => BindableKey.Escape,
            // Key.ImeConvert
            // Key.ImeNonConvert
            // Key.ImeAccept
            // Key.ImeModeChange
            Key.Space => BindableKey.Space,
            Key.PageUp => BindableKey.PageUp,
            // Key.Prior => BindableKey.PageDown,
            Key.PageDown => BindableKey.PageDown,
            // Key.Next => BindableKey.PageDown,
            Key.End => BindableKey.End,
            Key.Home => BindableKey.Home,
            Key.Left => BindableKey.Left,
            Key.Up => BindableKey.Up,
            Key.Right => BindableKey.Right,
            Key.Down => BindableKey.Down,
            Key.Select => BindableKey.Select,
            Key.Print => BindableKey.Print,
            Key.Execute => BindableKey.Execute,
            Key.PrintScreen => BindableKey.PrintScreen,
            // Key.Snapshot => BindableKey.PrintScreen,
            Key.Insert => BindableKey.Insert,
            Key.Delete => BindableKey.Delete,
            Key.Help => BindableKey.Help,
            Key.D0 => BindableKey.D0,
            Key.D1 => BindableKey.D1,
            Key.D2 => BindableKey.D2,
            Key.D3 => BindableKey.D3,
            Key.D4 => BindableKey.D4,
            Key.D5 => BindableKey.D5,
            Key.D6 => BindableKey.D6,
            Key.D7 => BindableKey.D7,
            Key.D8 => BindableKey.D8,
            Key.D9 => BindableKey.D9,
            Key.A => BindableKey.A,
            Key.B => BindableKey.B,
            Key.C => BindableKey.C,
            Key.D => BindableKey.D,
            Key.E => BindableKey.E,
            Key.F => BindableKey.F,
            Key.G => BindableKey.G,
            Key.H => BindableKey.H,
            Key.I => BindableKey.I,
            Key.J => BindableKey.J,
            Key.K => BindableKey.K,
            Key.L => BindableKey.L,
            Key.M => BindableKey.M,
            Key.N => BindableKey.N,
            Key.O => BindableKey.O,
            Key.P => BindableKey.P,
            Key.Q => BindableKey.Q,
            Key.R => BindableKey.R,
            Key.S => BindableKey.S,
            Key.T => BindableKey.T,
            Key.U => BindableKey.U,
            Key.V => BindableKey.V,
            Key.W => BindableKey.W,
            Key.X => BindableKey.X,
            Key.Y => BindableKey.Y,
            Key.Z => BindableKey.Z,
            Key.LWin => BindableKey.LeftWindows,
            Key.RWin => BindableKey.RightWindows,
            Key.Apps => BindableKey.Applications,
            Key.Sleep => BindableKey.Sleep,
            Key.NumPad0 => BindableKey.Numpad0,
            Key.NumPad1 => BindableKey.Numpad1,
            Key.NumPad2 => BindableKey.Numpad2,
            Key.NumPad3 => BindableKey.Numpad3,
            Key.NumPad4 => BindableKey.Numpad4,
            Key.NumPad5 => BindableKey.Numpad5,
            Key.NumPad6 => BindableKey.Numpad6,
            Key.NumPad7 => BindableKey.Numpad7,
            Key.NumPad8 => BindableKey.Numpad8,
            Key.NumPad9 => BindableKey.Numpad9,
            Key.Multiply => BindableKey.Multiply,
            Key.Add => BindableKey.Add,
            Key.Separator => BindableKey.Separator,
            Key.Subtract => BindableKey.Subtract,
            Key.Decimal => BindableKey.Decimal,
            Key.Divide => BindableKey.Divide,
            Key.F1 => BindableKey.F1,
            Key.F2 => BindableKey.F2,
            Key.F3 => BindableKey.F3,
            Key.F4 => BindableKey.F4,
            Key.F5 => BindableKey.F5,
            Key.F6 => BindableKey.F6,
            Key.F7 => BindableKey.F7,
            Key.F8 => BindableKey.F8,
            Key.F9 => BindableKey.F9,
            Key.F10 => BindableKey.F10,
            Key.F11 => BindableKey.F11,
            Key.F12 => BindableKey.F12,
            Key.F13 => BindableKey.F13,
            Key.F14 => BindableKey.F14,
            Key.F15 => BindableKey.F15,
            Key.F16 => BindableKey.F16,
            Key.F17 => BindableKey.F17,
            Key.F18 => BindableKey.F18,
            Key.F19 => BindableKey.F19,
            Key.F20 => BindableKey.F20,
            Key.F21 => BindableKey.F21,
            Key.F22 => BindableKey.F22,
            Key.F23 => BindableKey.F23,
            Key.F24 => BindableKey.F24,
            Key.NumLock => BindableKey.NumLock,
            Key.Scroll => BindableKey.ScrollLock,
            Key.LeftShift => BindableKey.LeftShift,
            Key.RightShift => BindableKey.RightShift,
            Key.LeftCtrl => BindableKey.LeftControl,
            Key.RightCtrl => BindableKey.RightControl,
            Key.LeftAlt => BindableKey.LeftAlt,
            Key.RightAlt => BindableKey.RightAlt,
            Key.BrowserBack => BindableKey.BrowserBack,
            Key.BrowserForward => BindableKey.BrowserForward,
            Key.BrowserRefresh => BindableKey.BrowserRefresh,
            Key.BrowserStop => BindableKey.BrowserStop,
            Key.BrowserSearch => BindableKey.BrowserSearch,
            Key.BrowserFavorites => BindableKey.BrowserFavorites,
            Key.BrowserHome => BindableKey.BrowserHome,
            Key.VolumeMute => BindableKey.VolumeMute,
            Key.VolumeDown => BindableKey.VolumeDown,
            Key.VolumeUp => BindableKey.VolumeUp,
            Key.MediaNextTrack => BindableKey.MediaNextTrack,
            Key.MediaPreviousTrack => BindableKey.MediaPreviousTrack,
            Key.MediaStop => BindableKey.MediaStop,
            Key.MediaPlayPause => BindableKey.MediaPlayPause,
            Key.LaunchMail => BindableKey.LaunchMail,
            Key.SelectMedia => BindableKey.SelectMedia,
            Key.LaunchApplication1 => BindableKey.LaunchApplication1,
            Key.LaunchApplication2 => BindableKey.LaunchApplication2,
            Key.Oem1 => BindableKey.Oem1,
            Key.OemPlus => BindableKey.OemPlus,
            Key.OemComma => BindableKey.OemComma,
            Key.OemMinus => BindableKey.OemMinus,
            Key.OemPeriod => BindableKey.OemPeriod,
            Key.OemQuestion => BindableKey.OemQuestion,
            // Key.Oem2 => BindableKey.Oem2,
            Key.OemTilde => BindableKey.OemTilde,
            // Key.Oem3 => BindableKey.Oem3,
            // Key.AbntC1
            // Key.AbntC2
            Key.OemOpenBrackets => BindableKey.OemOpenBracket,
            // Key.Oem4 => BindableKey.OemOpenBracket,
            Key.OemPipe => BindableKey.OemPipe,
            // Key.Oem5 => BindableKey.OemPipe,
            Key.OemCloseBrackets => BindableKey.OemCloseBracket,
            // Key.Oem6
            Key.OemQuotes => BindableKey.OemQuotes,
            // Key.Oem7 => BindableKey.OemQuotes,
            Key.Oem8 => BindableKey.Oem8,
            Key.OemBackslash => BindableKey.OemBackslash,
            // Key.Oem102 => BindableKey.OemBackslash,
            // Key.ImeProcessed
            // Key.System
            // Key.OemAttn
            // Key.DbeAlphanumeric
            // Key.OemFinish
            // Key.DbeKatakana
            // Key.DbeHiragana
            // Key.OemCopy
            // Key.DbeSbcsChar
            // Key.OemAuto
            // Key.DbeDbcsChar
            // Key.OemEnlw
            // Key.OemBackTab
            // Key.DbeRoman
            // Key.DbeNoRoman
            // Key.Attn
            // Key.CrSel
            // Key.DbeEnterWordRegisterMode
            // Key.ExSel
            // Key.DbeEnterImeConfigureMode
            // Key.EraseEof
            // Key.DbeFlushString
            // Key.Play
            // Key.DbeCodeInput
            // Key.DbeNoCodeInput
            // Key.Zoom
            // Key.NoName
            // Key.DbeDetermineString
            // Key.DbeEnterDialogConversionMode
            // Key.Pa1
            // Key.OemClear
            // Key.DeadCharProcessed
            // Key.FnLeftArrow
            // Key.FnRightArrow
            // Key.FnUpArrow
            // Key.FnDownArrow
            _ => BindableKey.None
        };
    }
}
