using System;
using TabletDriverLib.Interop.Converters;
using AvaloniaKey = Avalonia.Input.Key;
using NativeKey = TabletDriverLib.Interop.Input.Key;

namespace OpenTabletDriverGUI.Models
{
    public class AvaloniaKeyConverter : IConverter<AvaloniaKey, NativeKey>
    {
        private const int Alphabet = 21;
        private const int Number = 14;
        private const int NumpadNumber = 22;

        public AvaloniaKey Convert(NativeKey obj)
        {
            AvaloniaKey key;
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
                    key = (AvaloniaKey)obj - Alphabet;
                    break;
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
                    key = (AvaloniaKey)obj - Number;
                    break;
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
                    key = (AvaloniaKey)obj - NumpadNumber;
                    break;
                case NativeKey.Backspace:
                    key = AvaloniaKey.Back;
                    break;
                case NativeKey.PauseBreak:
                    key = AvaloniaKey.Pause;
                    break;
                case NativeKey.LeftControl:
                    key = AvaloniaKey.LeftCtrl;
                    break;
                case NativeKey.RightControl:
                    key = AvaloniaKey.RightCtrl;
                    break;
                case NativeKey.LeftMenu:
                    key = AvaloniaKey.LeftAlt;
                    break;
                case NativeKey.RightMenu:
                    key = AvaloniaKey.RightAlt;
                    break;
                case NativeKey.LeftSuper:
                    key = AvaloniaKey.LWin;
                    break;
                case NativeKey.RightSuper:
                    key = AvaloniaKey.RWin;
                    break;
                case NativeKey.NextTrack:
                    key = AvaloniaKey.MediaNextTrack;
                    break;
                case NativeKey.PreviousTrack:
                    key = AvaloniaKey.MediaPreviousTrack;
                    break;
                case NativeKey.Stop:
                    key = AvaloniaKey.MediaStop;
                    break;
                case NativeKey.PlayPause:
                    key = AvaloniaKey.MediaPlayPause;
                    break;
                case NativeKey.Semicolon:
                    key = AvaloniaKey.OemSemicolon;
                    break;
                case NativeKey.PlusEqual:
                    key = AvaloniaKey.OemPlus;
                    break;
                case NativeKey.Comma:
                    key = AvaloniaKey.OemComma;
                    break;
                case NativeKey.Minus:
                    key = AvaloniaKey.OemMinus;
                    break;
                case NativeKey.Period:
                    key = AvaloniaKey.OemPeriod;
                    break;
                case NativeKey.ForwardSlash:
                    key = AvaloniaKey.OemQuestion;
                    break;
                case NativeKey.Grave:
                    key = AvaloniaKey.OemTilde;
                    break;
                case NativeKey.LeftBracket:
                    key = AvaloniaKey.OemOpenBrackets;
                    break;
                case NativeKey.Backslash:
                    key = AvaloniaKey.OemBackslash;
                    break;
                case NativeKey.RightBracket:
                    key = AvaloniaKey.OemCloseBrackets;
                    break;
                case NativeKey.Quote:
                    key = AvaloniaKey.OemQuotes;
                    break;
                default:
                    key = Enum.Parse<AvaloniaKey>(obj.GetType().Name);
                    break;
            }
            return key;
        }

        public NativeKey Convert(AvaloniaKey obj)
        {
            NativeKey key;
            switch (obj)
            {
                case AvaloniaKey.None:
                    key = 0;
                    break;
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
                    key = (NativeKey)obj + Alphabet;
                    break;
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
                    key = (NativeKey)obj + Number;
                    break;
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
                    key = (NativeKey)obj + NumpadNumber;
                    break;
                case AvaloniaKey.Back:
                    key = NativeKey.Backspace;
                    break;
                case AvaloniaKey.Pause:
                    key = NativeKey.PauseBreak;
                    break;
                case AvaloniaKey.LeftCtrl:
                    key = NativeKey.LeftControl;
                    break;
                case AvaloniaKey.RightCtrl:
                    key = NativeKey.RightControl;
                    break;
                case AvaloniaKey.LeftAlt:
                    key = NativeKey.LeftMenu;
                    break;
                case AvaloniaKey.RightAlt:
                    key = NativeKey.RightMenu;
                    break;
                case AvaloniaKey.LWin:
                    key = NativeKey.LeftSuper;
                    break;
                case AvaloniaKey.RWin:
                    key = NativeKey.RightSuper;
                    break;
                case AvaloniaKey.MediaNextTrack:
                    key = NativeKey.NextTrack;
                    break;
                case AvaloniaKey.MediaPreviousTrack:
                    key = NativeKey.PreviousTrack;
                    break;
                case AvaloniaKey.MediaStop:
                    key = NativeKey.Stop;
                    break;
                case AvaloniaKey.MediaPlayPause:
                    key = NativeKey.PlayPause;
                    break;
                case AvaloniaKey.OemSemicolon:
                    key = NativeKey.Semicolon;
                    break;
                case AvaloniaKey.OemPlus:
                    key = NativeKey.PlusEqual;
                    break;
                case AvaloniaKey.OemComma:
                    key = NativeKey.Comma;
                    break;
                case AvaloniaKey.OemMinus:
                    key = NativeKey.Minus;
                    break;
                case AvaloniaKey.OemPeriod:
                    key = NativeKey.Period;
                    break;
                case AvaloniaKey.OemQuestion:
                    key = NativeKey.ForwardSlash;
                    break;
                case AvaloniaKey.OemTilde:
                    key = NativeKey.Grave;
                    break;
                case AvaloniaKey.OemOpenBrackets:
                    key = NativeKey.LeftBracket;
                    break;
                case AvaloniaKey.OemPipe: // TODO: Verify
                case AvaloniaKey.OemBackslash:
                    key = NativeKey.Backslash;
                    break;
                case AvaloniaKey.OemCloseBrackets:
                    key = NativeKey.RightBracket;
                    break;
                case AvaloniaKey.OemQuotes:
                    key = NativeKey.Quote;
                    break;
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
                    key = Enum.Parse<NativeKey>(obj.GetType().Name);
                    break;
            }
            return key;
        }
    }
}