using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Eto.Forms;
using TabletDriverLib;
using TabletDriverLib.Binding;
using TabletDriverPlugin;

namespace OpenTabletDriver.UX.Windows
{
    public class AdvancedBindingEditorDialog : Dialog<BindingReference>
    {
        public AdvancedBindingEditorDialog(BindingReference currentBinding = null)
        {
            Title = "Advanced Binding Editor";
            Result = currentBinding;
            Padding = 5;

            BindingName = currentBinding.Binding?.Path;
            BindingProperty = currentBinding.BindingProperty;

            var bindingTypes = PluginManager.GetChildTypes<TabletDriverPlugin.IBinding>();

            var bindingName = GetControl(
                "Name",
                () => BindingName,
                (o) => BindingName = o
            );
            var bindingProperty = GetControl(
                "Property",
                () => BindingProperty,
                (o) => BindingProperty = o
            );
            
            var bindingPanel = new StackLayout
            {
                Orientation = Orientation.Vertical,
                Spacing = 5,
                Items = 
                {
                    new StackLayoutItem(bindingName)
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch
                    },
                    new StackLayoutItem(bindingProperty)
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch
                    }
                }
            };

            var clearCommand = new Command { MenuText = "Clear" };
            clearCommand.Executed += ClearBinding;

            var applyCommand = new Command { MenuText = "Apply" };
            applyCommand.Executed += ApplyBinding;
            
            var clearButton = new Button
            {
                Text = "Clear",
                Command = clearCommand,
            };

            var applyButton = new Button
            {
                Text = "Apply",
                Command = applyCommand
            };
            
            var buttonPanel = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5,
                Items = 
                { 
                    new StackLayoutItem(clearButton)
                    {
                        Expand = true
                    },
                    new StackLayoutItem(applyButton)
                    {
                        Expand = true
                    }
                }
            };

            this.Content = new StackLayout
            {
                Width = 300,
                Height = 250,
                Items = 
                {
                    new StackLayoutItem(bindingPanel)
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        Expand = true
                    },
                    new StackLayoutItem(buttonPanel)
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch
                    }
                }
            };
        }

        private string _bindingName, _bindingProperty;

        private string BindingName
        {
            set => _bindingName = value;
            get => _bindingName;
        }

        private string BindingProperty
        {
            set => _bindingProperty = value;
            get => _bindingProperty;
        }

        private void ClearBinding(object sender, EventArgs e)
        {
            Return(string.Empty);
        }

        private void ApplyBinding(object sender, EventArgs e)
        {
            Return($"{BindingName}: {BindingProperty}");
        }

        public void Return(string result)
        {
            Close(BindingReference.FromString(result));
        }

        private GroupBox GetControl(string header, Func<string> getValue, Action<string> setValue)
        {
            var textBox = new TextBox();
            textBox.TextBinding.Bind(getValue, setValue);

            return new GroupBox
            {
                Text = header,
                Padding = App.GroupBoxPadding,
                Content = textBox
            };
        }
    }
}