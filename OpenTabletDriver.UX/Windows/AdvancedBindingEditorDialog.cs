using System;
using System.Linq;
using Eto.Forms;
using OpenTabletDriver.Binding;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Reflection;
using IBinding = OpenTabletDriver.Plugin.IBinding;

namespace OpenTabletDriver.UX.Windows
{
    public class AdvancedBindingEditorDialog : Dialog<BindingReference>
    {
        public AdvancedBindingEditorDialog(BindingReference currentBinding = null)
        {
            Title = "Advanced Binding Editor";
            Result = currentBinding;
            Padding = 5;

            BindingPath = currentBinding.Binding?.Path;
            BindingProperty = currentBinding.BindingProperty;

            var bindingTypes = PluginManager.GetChildTypes<OpenTabletDriver.Plugin.IBinding>();

            var bindingPath = GetBindingSelector(
                () => BindingPath,
                (o) => BindingPath = o
            );
            var bindingProperty = GetPropertySelector(
                (ComboBox)bindingPath.Content,
                () => BindingProperty,
                (o) => BindingProperty = o
            );
            
            var bindingPanel = new StackLayout
            {
                Orientation = Orientation.Vertical,
                Spacing = 5,
                Items = 
                {
                    new StackLayoutItem(bindingPath)
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

        private string _bindingPath, _bindingProperty;

        private string BindingPath
        {
            set => _bindingPath = value;
            get => _bindingPath;
        }

        private string BindingProperty
        {
            set => _bindingProperty = value;
            get => _bindingProperty;
        }

        private void ClearBinding(object sender, EventArgs e)
        {
            Close(BindingReference.None);
        }

        private void ApplyBinding(object sender, EventArgs e)
        {
            Close(new BindingReference(BindingPath, BindingProperty));
        }

        private GroupBox GetBindingSelector(Func<string> getValue, Action<string> setValue)
        {
            var selector = new ComboBox();
            var items = from type in PluginManager.GetChildTypes<IBinding>()
                where !type.IsInterface
                let pluginRef = new PluginReference(type)
                select new ListItem
                {
                    Text = pluginRef.Name,
                    Key = pluginRef.Path
                };

            selector.Items.AddRange(items);
            selector.SelectedKeyBinding.Bind(
                getValue,
                setValue
            );

            return new GroupBox
            {
                Text = "Binding Type",
                Padding = App.GroupBoxPadding,
                Content = selector
            };
        }

        private Control GetPropertySelector(ComboBox bindingSelector, Func<string> getValue, Action<string> setValue)
        {
            var selector = new ComboBox();
            var generic = new TextBox();
            generic.TextBinding.Bind(getValue, setValue);

            var groupBox = new GroupBox
            {
                Text = "Binding Property",
                Padding = App.GroupBoxPadding,
                Content = generic
            };

            void updateControl()
            {
                var pluginRef = new PluginReference(BindingPath);
                var type = pluginRef.GetTypeReference<IBinding>();
                if (typeof(IValidateBinding).IsAssignableFrom(type))
                {
                    selector.Items.Clear();
                    var binding = pluginRef.Construct<IValidateBinding>();
                    var items = from item in binding.ValidProperties
                        select new ListItem
                        {
                            Text = item,
                            Key = item
                        };

                    selector.Items.AddRange(items);
                    selector.SelectedKeyBinding.Bind(
                        getValue,
                        setValue
                    );
                    
                    groupBox.Content = selector;
                }
                else
                {
                    groupBox.Content = generic;
                }
            }
            bindingSelector.SelectedKeyChanged += (sender, e) => updateControl();
            updateControl();
            return groupBox;
        }
    }
}