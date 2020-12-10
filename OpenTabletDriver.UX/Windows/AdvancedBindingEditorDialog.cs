using System;
using System.Linq;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.UX.Controls.Generic;
using IBinding = OpenTabletDriver.Plugin.IBinding;

namespace OpenTabletDriver.UX.Windows
{
    public class AdvancedBindingEditorDialog : Dialog<PluginSettingStore>
    {
        public AdvancedBindingEditorDialog(PluginSettingStore currentBinding = null)
        {
            Title = "Advanced Binding Editor";
            Result = currentBinding;
            Padding = 5;

            BindingPath = currentBinding?.Path;
            BindingProperty = currentBinding?["Property"]?.GetValue<string>();

            var bindingTypes = AppInfo.PluginManager.GetChildTypes<OpenTabletDriver.Plugin.IBinding>();

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
            Close(null);
        }

        private void ApplyBinding(object sender, EventArgs e)
        {
            var binding = AppInfo.PluginManager.ConstructObject<IBinding>(BindingPath);
            binding.Property = BindingProperty;
            Close(new PluginSettingStore(binding));
        }

        private Group GetBindingSelector(Func<string> getValue, Action<string> setValue)
        {
            var selector = new ComboBox();
            var items = from type in AppInfo.PluginManager.GetChildTypes<IBinding>()
                where !type.IsInterface
                let pluginRef = AppInfo.PluginManager.GetPluginReference(type)
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

            return new Group("Binding Type", selector, Orientation.Horizontal);
        }

        private Control GetPropertySelector(ComboBox bindingSelector, Func<string> getValue, Action<string> setValue)
        {
            var selector = new ComboBox();
            var generic = new TextBox();
            generic.TextBinding.Bind(getValue, setValue);

            var group = new Group("Binding Property", generic);

            void updateControl()
            {
                var pluginRef = AppInfo.PluginManager.GetPluginReference(BindingPath);
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
                    
                    group.Content = selector;
                }
                else
                {
                    group.Content = generic;
                }
            }
            bindingSelector.SelectedKeyChanged += (sender, e) => updateControl();
            updateControl();
            return group;
        }
    }
}
