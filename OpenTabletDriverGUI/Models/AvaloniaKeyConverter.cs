using System;
using System.Text.RegularExpressions;
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
            var name = Enum.GetName(typeof(NativeKey), obj);
            if (name.Length == 1 && Regex.IsMatch(name, @"[A-Z]"))
                return ConvertAlphabet(obj);
            else if (Regex.IsMatch(name, @"D[0-9]"))
                return ConvertNumber(obj);
            else if (Regex.IsMatch(name, @"Num[Pp]ad\d"))
                return ConvertNumpadNumber(obj);
            
            switch (obj)
            {
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
            var name = Enum.GetName(typeof(NativeKey), obj);
            if (name.Length == 1 && Regex.IsMatch(name, @"[A-Z]{0-1}"))
                return ConvertAlphabet(obj);
            else if (Regex.IsMatch(name, @"D[0-9]"))
                return ConvertNumber(obj);
            else if (Regex.IsMatch(name, @"Num[Pp]ad\d"))
                return ConvertNumpadNumber(obj);

            switch (obj)
            {
                case AvaloniaKey.None:
                    return 0;
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