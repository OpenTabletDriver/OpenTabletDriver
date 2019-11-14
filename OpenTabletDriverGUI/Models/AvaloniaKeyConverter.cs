using System;
using TabletDriverLib.Interop.Converters;
using AvaloniaKey = Avalonia.Input.Key;
using NativeKey = TabletDriverLib.Interop.Input.Key;

namespace OpenTabletDriverGUI.Models
{
    public class AvaloniaKeyConverter : IConverter<AvaloniaKey, NativeKey>
    {
        private const int AlphabetOffset = 21;
        private const int NumberOffset = 14;
        private const int NumpadNumberOffset = 22;

        public AvaloniaKey Convert(NativeKey obj)
        {
            switch (obj)
            {
                case NativeKey.A:
                case NativeKey.B:
                case NativeKey.C:
                case NativeKey.D:
                case NativeKey.E:
                case NativeKey.F:
                case NativeKey.G:
                case NativeKey.H:
                case NativeKey.I:
                case NativeKey.J:
                case NativeKey.K:
                case NativeKey.L:
                case NativeKey.M:
                case NativeKey.N:
                case NativeKey.O:
                case NativeKey.P:
                case NativeKey.Q:
                case NativeKey.R:
                case NativeKey.S:
                case NativeKey.T:
                case NativeKey.U:
                case NativeKey.V:
                case NativeKey.W:
                case NativeKey.X:
                case NativeKey.Y:
                case NativeKey.Z:
                    return ConvertAlphabet(obj);
                case NativeKey.Zero:
                case NativeKey.One:
                case NativeKey.Two:
                case NativeKey.Three:
                case NativeKey.Four:
                case NativeKey.Five:
                case NativeKey.Six:
                case NativeKey.Seven:
                case NativeKey.Eight:
                case NativeKey.Nine:
                    return ConvertNumber(obj);
                case NativeKey.Numpad0:
                case NativeKey.Numpad1:
                case NativeKey.Numpad2:
                case NativeKey.Numpad3:
                case NativeKey.Numpad4:
                case NativeKey.Numpad5:
                case NativeKey.Numpad6:
                case NativeKey.Numpad7:
                case NativeKey.Numpad8:
                case NativeKey.Numpad9:
                    return ConvertNumpadNumber(obj);
                case NativeKey.Backspace:
                    return AvaloniaKey.Back;
                case NativeKey.PauseBreak:
                    return AvaloniaKey.Pause;
                case NativeKey.LeftControl:
                    return AvaloniaKey.LeftCtrl;
                case NativeKey.RightControl:
                    return AvaloniaKey.RightCtrl;
                case NativeKey.LeftMenu:
                    return AvaloniaKey.LeftAlt;
                case NativeKey.RightMenu:
                    return AvaloniaKey.RightAlt;
                case NativeKey.LeftSuper:
                    return AvaloniaKey.LWin;
                case NativeKey.RightSuper:
                    return AvaloniaKey.RWin;
                case NativeKey.NextTrack:
                    return AvaloniaKey.MediaNextTrack;
                case NativeKey.PreviousTrack:
                    return AvaloniaKey.MediaPreviousTrack;
                case NativeKey.Stop:
                    return AvaloniaKey.MediaStop;
                case NativeKey.PlayPause:
                    return AvaloniaKey.MediaPlayPause;
                case NativeKey.Semicolon:
                    return AvaloniaKey.OemSemicolon;
                case NativeKey.PlusEqual:
                    return AvaloniaKey.OemPlus;
                case NativeKey.Comma:
                    return AvaloniaKey.OemComma;
                case NativeKey.Minus:
                    return AvaloniaKey.OemMinus;
                case NativeKey.Period:
                    return AvaloniaKey.OemPeriod;
                case NativeKey.ForwardSlash:
                    return AvaloniaKey.OemQuestion;
                case NativeKey.Grave:
                    return AvaloniaKey.OemTilde;
                case NativeKey.LeftBracket:
                    return AvaloniaKey.OemOpenBrackets;
                case NativeKey.Backslash:
                    return AvaloniaKey.OemBackslash;
                case NativeKey.RightBracket:
                    return AvaloniaKey.OemCloseBrackets;
                case NativeKey.Quote:
                    return AvaloniaKey.OemQuotes;
                default:
                    return Enum.Parse<AvaloniaKey>(obj.GetType().Name);
            }
        }

        public NativeKey Convert(AvaloniaKey obj)
        {
            switch (obj)
            {
                case AvaloniaKey.None:
                    return 0;
                case AvaloniaKey.A:
                case AvaloniaKey.B:
                case AvaloniaKey.C:
                case AvaloniaKey.D:
                case AvaloniaKey.E:
                case AvaloniaKey.F:
                case AvaloniaKey.G:
                case AvaloniaKey.H:
                case AvaloniaKey.I:
                case AvaloniaKey.J:
                case AvaloniaKey.K:
                case AvaloniaKey.L:
                case AvaloniaKey.M:
                case AvaloniaKey.N:
                case AvaloniaKey.O:
                case AvaloniaKey.P:
                case AvaloniaKey.Q:
                case AvaloniaKey.R:
                case AvaloniaKey.S:
                case AvaloniaKey.T:
                case AvaloniaKey.U:
                case AvaloniaKey.V:
                case AvaloniaKey.W:
                case AvaloniaKey.X:
                case AvaloniaKey.Y:
                case AvaloniaKey.Z:
                    return ConvertAlphabet(obj);
                case AvaloniaKey.D0:
                case AvaloniaKey.D1:
                case AvaloniaKey.D2:
                case AvaloniaKey.D3:
                case AvaloniaKey.D4:
                case AvaloniaKey.D5:
                case AvaloniaKey.D6:
                case AvaloniaKey.D7:
                case AvaloniaKey.D8:
                case AvaloniaKey.D9:
                    return ConvertNumber(obj);
                case AvaloniaKey.NumPad0:
                case AvaloniaKey.NumPad1:
                case AvaloniaKey.NumPad2:
                case AvaloniaKey.NumPad3:
                case AvaloniaKey.NumPad4:
                case AvaloniaKey.NumPad5:
                case AvaloniaKey.NumPad6:
                case AvaloniaKey.NumPad7:
                case AvaloniaKey.NumPad8:
                case AvaloniaKey.NumPad9:
                    return ConvertNumpadNumber(obj);
                case AvaloniaKey.Back:
                    return NativeKey.Backspace;
                case AvaloniaKey.Pause:
                    return NativeKey.PauseBreak;
                case AvaloniaKey.LeftCtrl:
                    return NativeKey.LeftControl;
                case AvaloniaKey.RightCtrl:
                    return NativeKey.RightControl;
                case AvaloniaKey.LeftAlt:
                    return NativeKey.LeftMenu;
                case AvaloniaKey.RightAlt:
                    return NativeKey.RightMenu;
                case AvaloniaKey.LWin:
                    return NativeKey.LeftSuper;
                case AvaloniaKey.RWin:
                    return NativeKey.RightSuper;
                case AvaloniaKey.MediaNextTrack:
                    return NativeKey.NextTrack;
                case AvaloniaKey.MediaPreviousTrack:
                    return NativeKey.PreviousTrack;
                case AvaloniaKey.MediaStop:
                    return NativeKey.Stop;
                case AvaloniaKey.MediaPlayPause:
                    return NativeKey.PlayPause;
                case AvaloniaKey.OemSemicolon:
                    return NativeKey.Semicolon;
                case AvaloniaKey.OemPlus:
                    return NativeKey.PlusEqual;
                case AvaloniaKey.OemComma:
                    return NativeKey.Comma;
                case AvaloniaKey.OemMinus:
                    return NativeKey.Minus;
                case AvaloniaKey.OemPeriod:
                    return NativeKey.Period;
                case AvaloniaKey.OemQuestion:
                    return NativeKey.ForwardSlash;
                case AvaloniaKey.OemTilde:
                    return NativeKey.Grave;
                case AvaloniaKey.OemOpenBrackets:
                    return NativeKey.LeftBracket;
                case AvaloniaKey.OemPipe: // TODO: Verify
                case AvaloniaKey.OemBackslash:
                    return NativeKey.Backslash;
                case AvaloniaKey.OemCloseBrackets:
                    return NativeKey.RightBracket;
                case AvaloniaKey.OemQuotes:
                    return NativeKey.Quote;
                // Disabled keys
                case AvaloniaKey.Help:
                case AvaloniaKey.PrintScreen:
                case AvaloniaKey.Sleep:
                case AvaloniaKey.Apps:
                case AvaloniaKey.LaunchApplication1:
                case AvaloniaKey.LaunchApplication2:
                case AvaloniaKey.LaunchMail:
                case AvaloniaKey.BrowserBack:
                case AvaloniaKey.BrowserForward:
                case AvaloniaKey.BrowserRefresh:
                case AvaloniaKey.BrowserFavorites:
                case AvaloniaKey.BrowserHome:
                case AvaloniaKey.BrowserStop:
                case AvaloniaKey.BrowserSearch:
                case AvaloniaKey.Scroll:
                case AvaloniaKey.SelectMedia:
                case AvaloniaKey.ImeProcessed:
                case AvaloniaKey.System:
                case AvaloniaKey.OemAttn:
                case AvaloniaKey.OemFinish:
                case AvaloniaKey.DbeHiragana:
                case AvaloniaKey.DbeSbcsChar:
                case AvaloniaKey.DbeDbcsChar:
                case AvaloniaKey.OemBackTab:
                case AvaloniaKey.DbeNoRoman:
                case AvaloniaKey.CrSel:
                case AvaloniaKey.ExSel:
                case AvaloniaKey.EraseEof:
                case AvaloniaKey.Play:
                case AvaloniaKey.NoName:
                case AvaloniaKey.DbeEnterDialogConversionMode:
                case AvaloniaKey.OemClear:
                case AvaloniaKey.DeadCharProcessed:
                case AvaloniaKey.FnLeftArrow:
                case AvaloniaKey.FnRightArrow:
                case AvaloniaKey.FnUpArrow:
                case AvaloniaKey.FnDownArrow:
                    // These keys are not available in the Native Key enum.
                    throw new InvalidCastException();
                default:
                    // Names are the same, should parse without issue.
                    return Enum.Parse<NativeKey>(obj.GetType().Name);
            }
        }

        private AvaloniaKey ConvertAlphabet(NativeKey key)
        {
            return (AvaloniaKey)key - AlphabetOffset;
        }

        private NativeKey ConvertAlphabet(AvaloniaKey key)
        {
            return (NativeKey)key + AlphabetOffset;
        }

        private AvaloniaKey ConvertNumber(NativeKey key)
        {
            return (AvaloniaKey)key - NumberOffset;
        }

        private NativeKey ConvertNumber(AvaloniaKey key)
        {
            return (NativeKey)key + NumberOffset;
        }

        private AvaloniaKey ConvertNumpadNumber(NativeKey key)
        {
            return (AvaloniaKey)key - NumpadNumberOffset;
        }

        private NativeKey ConvertNumpadNumber(AvaloniaKey key)
        {
            return (NativeKey)key + NumpadNumberOffset;
        }
    }
}