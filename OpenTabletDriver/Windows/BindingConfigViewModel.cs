using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using ReactiveUI;
using TabletDriverLib;
using TabletDriverLib.Binding;
using TabletDriverPlugin;
using TabletDriverPlugin.Attributes;

using AvaloniaKey = Avalonia.Input.Key;
using AvaloniaMouse = Avalonia.Input.MouseButton;

namespace OpenTabletDriver.Windows
{
    public class BindingConfigViewModel : ViewModelBase
    {
        public BindingConfigViewModel(string binding) : this()
        {
            Binding = binding;
            var tokens = Binding?.Split(", ", 2) ?? new string[0];

            if (tokens.Length == 2)
            {
                if (tokens[0] == typeof(MouseBinding).FullName)
                    (MouseBindingControl as TextBlock).Text = tokens[1];
                else if (tokens[0] == typeof(KeyBinding).FullName)
                    (KeyBindingControl as TextBox).Text = tokens[1];
                else
                {
                    SelectedCustom = BindingTypes.FirstOrDefault(t => t.FullName == tokens[0]);
                    Property = tokens[1];
                }
            }
        }

        public BindingConfigViewModel()
        {
            RefreshTypes();
            CreateMouseBindingView();
            CreateKeyBindingView();
        }

        private string _binding;
        public string Binding
        {
            set => this.RaiseAndSetIfChanged(ref _binding, value);
            get => _binding;
        }

        private ObservableCollection<Type> _bindingTypes;
        public ObservableCollection<Type> BindingTypes
        {
            set => this.RaiseAndSetIfChanged(ref _bindingTypes, value);
            get => _bindingTypes;
        }

        private IControl _mouseCtrl, _keyCtrl;

        public IControl MouseBindingControl
        {
            set => this.RaiseAndSetIfChanged(ref _mouseCtrl, value);
            get => _mouseCtrl;
        }

        public IControl KeyBindingControl
        {
            set => this.RaiseAndSetIfChanged(ref _keyCtrl, value);
            get => _keyCtrl;
        }

        private Type _custom;
        public Type SelectedCustom
        {
            set
            {
                var type = this.RaiseAndSetIfChanged(ref _custom, value);
                Binding = type?.FullName + ", " + Property ?? string.Empty;
            }
            get => _custom;
        }

        private string _property;
        public string Property
        {
            set
            {
                var property = this.RaiseAndSetIfChanged(ref _property, value);
                Binding = SelectedCustom?.FullName + ", " + property ?? string.Empty;
            }
            get => _property;
        }

        public void RefreshTypes()
        {
            var types = from type in PluginManager.GetChildTypes<IBinding>()
                        where !type.IsInterface
                        where !type.GetCustomAttributes(false).Any(a => a.GetType() == typeof(PluginIgnoreAttribute))
                        where type != typeof(MouseBinding)
                        where type != typeof(KeyBinding)
                        select type;
            BindingTypes = new ObservableCollection<Type>(types);
        }

#pragma warning disable 618

        public void CreateMouseBindingView()
        {
            var ctrl = new TextBlock { Classes = new Classes("bind") };
            ctrl.PointerPressed += PointerPressedEventHandler;
            MouseBindingControl = ctrl;

            void PointerPressedEventHandler(object sender, Avalonia.Input.PointerPressedEventArgs e)
            {
                var mb = GetMouseButton(e.MouseButton);
                Binding = typeof(MouseBinding).FullName + ", " + mb;
                ctrl.Text = mb;
            }
        }

#pragma warning restore 618

        public void CreateKeyBindingView()
        {
            var ctrl = new TextBox { Classes = new Classes("bind") };
            ctrl.KeyDown += KeyDownEventHandler;
            KeyBindingControl = ctrl;

            void KeyDownEventHandler(object sender, Avalonia.Input.KeyEventArgs e)
            {
                if (AvaloniaToX11Key.TryGetValue(e.Key, out var keyName))
                {
                    Binding = typeof(KeyBinding).FullName + ", " + keyName;
                    ctrl.Text = keyName;
                }
                else
                    ctrl.Text = "Unsupported key binding.";
            }
        }

        public void ClearBinding()
        {
            Binding = string.Empty;
            CreateMouseBindingView();
            CreateKeyBindingView();
            SelectedCustom = null;
            Property = null;
        }

        private static string GetMouseButton(AvaloniaMouse m)
        {
            switch (m)
            {
                case AvaloniaMouse.Left:
                    return "Left";
                case AvaloniaMouse.Middle:
                    return "Middle";
                case AvaloniaMouse.Right:
                    return "Right";
                case AvaloniaMouse.None:
                default:
                    return "None";
            }
        }

        private static readonly Dictionary<AvaloniaKey, string> AvaloniaToX11Key = new Dictionary<AvaloniaKey, string>
        {
            { AvaloniaKey.Cancel, "Cancel" },
            { AvaloniaKey.Back, "BackSpace" },
            { AvaloniaKey.Tab, "Tab" },
            { AvaloniaKey.LineFeed, "Linefeed" },
            { AvaloniaKey.Clear, "Clear" },
            { AvaloniaKey.Return, "Return" },
            { AvaloniaKey.Pause, "Pause" },
            { AvaloniaKey.CapsLock, "Caps_Lock" },
            { AvaloniaKey.Escape, "Escape" },
            { AvaloniaKey.Space, "space" },
            { AvaloniaKey.Prior, "Prior" },
            { AvaloniaKey.PageDown, "Page_Down" },
            { AvaloniaKey.End, "End" },
            { AvaloniaKey.Home, "Home" },
            { AvaloniaKey.Left, "Left" },
            { AvaloniaKey.Up, "Up" },
            { AvaloniaKey.Right, "Right" },
            { AvaloniaKey.Down, "Down" },
            { AvaloniaKey.Select, "Select" },
            { AvaloniaKey.Print, "Print" },
            { AvaloniaKey.Execute, "Execute" },
            { AvaloniaKey.Insert, "Insert" },
            { AvaloniaKey.Delete, "Delete" },
            { AvaloniaKey.Help, "Help" },
            { AvaloniaKey.A, "A" },
            { AvaloniaKey.B, "B" },
            { AvaloniaKey.C, "C" },
            { AvaloniaKey.D, "D" },
            { AvaloniaKey.E, "E" },
            { AvaloniaKey.F, "F" },
            { AvaloniaKey.G, "G" },
            { AvaloniaKey.H, "H" },
            { AvaloniaKey.I, "I" },
            { AvaloniaKey.J, "J" },
            { AvaloniaKey.K, "K" },
            { AvaloniaKey.L, "L" },
            { AvaloniaKey.M, "M" },
            { AvaloniaKey.N, "N" },
            { AvaloniaKey.O, "O" },
            { AvaloniaKey.P, "P" },
            { AvaloniaKey.Q, "Q" },
            { AvaloniaKey.R, "R" },
            { AvaloniaKey.S, "S" },
            { AvaloniaKey.T, "T" },
            { AvaloniaKey.U, "U" },
            { AvaloniaKey.V, "V" },
            { AvaloniaKey.W, "W" },
            { AvaloniaKey.X, "X" },
            { AvaloniaKey.Y, "Y" },
            { AvaloniaKey.Z, "Z" },
            { AvaloniaKey.LWin , "Super_L" },
            { AvaloniaKey.RWin , "Super_R" },
            { AvaloniaKey.Apps, "Menu" },
            { AvaloniaKey.NumPad0, "KP_0" },
            { AvaloniaKey.NumPad1, "KP_1" },
            { AvaloniaKey.NumPad2, "KP_2" },
            { AvaloniaKey.NumPad3, "KP_3" },
            { AvaloniaKey.NumPad4, "KP_4" },
            { AvaloniaKey.NumPad5, "KP_5" },
            { AvaloniaKey.NumPad6, "KP_6" },
            { AvaloniaKey.NumPad7, "KP_7" },
            { AvaloniaKey.NumPad8, "KP_8" },
            { AvaloniaKey.NumPad9, "KP_9" },
            { AvaloniaKey.Multiply, "multiply" },
            { AvaloniaKey.Add, "KP_Add" },
            { AvaloniaKey.Subtract, "KP_Subtract" },
            { AvaloniaKey.Decimal, "KP_Decimal" },
            { AvaloniaKey.Divide, "KP_Divide" },
            { AvaloniaKey.F1, "F1" },
            { AvaloniaKey.F2, "F2" },
            { AvaloniaKey.F3, "F3" },
            { AvaloniaKey.F4, "F4" },
            { AvaloniaKey.F5, "F5" },
            { AvaloniaKey.F6, "F6" },
            { AvaloniaKey.F7, "F7" },
            { AvaloniaKey.F8, "F8" },
            { AvaloniaKey.F9, "F9" },
            { AvaloniaKey.F10, "F10" },
            { AvaloniaKey.F11, "F11" },
            { AvaloniaKey.F12, "F12" },
            { AvaloniaKey.F13, "L3" },
            { AvaloniaKey.F14, "F14" },
            { AvaloniaKey.F15, "L5" },
            { AvaloniaKey.F16, "F16" },
            { AvaloniaKey.F17, "F17" },
            { AvaloniaKey.F18, "L8" },
            { AvaloniaKey.F19, "L9" },
            { AvaloniaKey.F20, "L10" },
            { AvaloniaKey.F21, "R1" },
            { AvaloniaKey.F22, "R2" },
            { AvaloniaKey.F23, "F23" },
            { AvaloniaKey.F24, "R4" },
            { AvaloniaKey.NumLock, "Num_Lock" },
            { AvaloniaKey.Scroll, "Scroll_Lock" },
            { AvaloniaKey.LeftShift, "Shift_L" },
            { AvaloniaKey.RightShift, "Shift_R" },
            { AvaloniaKey.LeftCtrl, "Control_L" },
            { AvaloniaKey.RightCtrl, "Control_R" },
            { AvaloniaKey.LeftAlt, "Alt_L" },
            { AvaloniaKey.RightAlt, "Alt_R" },
            { AvaloniaKey.OemMinus, "minus" },
            { AvaloniaKey.OemPlus, "plus" },
            { AvaloniaKey.OemOpenBrackets, "bracketleft" },
            { AvaloniaKey.OemCloseBrackets, "bracketright" },
            { AvaloniaKey.OemPipe, "backslash" },
            { AvaloniaKey.OemSemicolon, "semicolon" },
            { AvaloniaKey.OemQuotes, "apostrophe" },
            { AvaloniaKey.OemComma, "comma" },
            { AvaloniaKey.OemPeriod, "period" },
            { AvaloniaKey.Oem2, "slash" },
            { AvaloniaKey.OemTilde, "grave" },
            { AvaloniaKey.D1, "XK_1" },
            { AvaloniaKey.D2, "XK_2" },
            { AvaloniaKey.D3, "XK_3" },
            { AvaloniaKey.D4, "XK_4" },
            { AvaloniaKey.D5, "XK_5" },
            { AvaloniaKey.D6, "XK_6" },
            { AvaloniaKey.D7, "XK_7" },
            { AvaloniaKey.D8, "XK_8" },
            { AvaloniaKey.D9, "XK_9" },
            { AvaloniaKey.D0, "XK_0" }
        };
    }
}