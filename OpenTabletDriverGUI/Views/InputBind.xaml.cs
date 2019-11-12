using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Models;
using OpenTabletDriverGUI.Models;
using TabletDriverLib.Interop.Input;
using Key = TabletDriverLib.Interop.Input.Key;

namespace OpenTabletDriverGUI.Views
{
    public class InputBind : Window
    {
        public InputBind()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            var input = this.Find<TextBlock>("Input");
            input.KeyDown += new EventHandler<KeyEventArgs>(KeyPressed);
        }

        private AvaloniaKeyConverter Converter = new AvaloniaKeyConverter();

        public Key Result { private set; get; } = 0;

        private void KeyPressed(object sender, KeyEventArgs e)
        {
            try
            {
                Result = Converter.Convert(e.Key);
                this.Close();
            }
            catch (InvalidCastException castEx)
            {
                var msgBox = new MessageBoxCustomParams
                {
                    CanResize = false,
                    ShowInCenter = true,
                    ContentTitle = "Error: Invalid Key",
                    ContentHeader = $"{castEx.GetType().Name}",
                    ContentMessage = "Unable to bind this key. Report this to the GitHub repository!",
                    Icon = MessageBox.Avalonia.Enums.Icon.Error,
                    ButtonDefinitions = new List<ButtonDefinition>()
                    {
                        new ButtonDefinition { Name = "OK" }
                    }
                };
                var errWindow = MessageBoxWindow.CreateCustomWindow(msgBox);
                errWindow.ShowDialog(this);
            }
        }
    }
}