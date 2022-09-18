using System.Collections.Immutable;
using Eto.Forms;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.UX.Components;
using OpenTabletDriver.UX.ViewModels;

namespace OpenTabletDriver.UX.Dialogs
{
    public sealed class AdvancedBindingDialog : Dialog<DialogModel<PluginSettings>>
    {
        public AdvancedBindingDialog(IControlBuilder controlBuilder, IPluginFactory pluginFactory, DirectBinding<PluginSettings?> binding)
        {
            Title = "Select a binding...";

            Width = 500;
            Height = 400;

            var initialSettings = binding.DataValue;

            var typesQuery = from item in pluginFactory.GetMatchingTypes(typeof(IBinding))
                orderby item.GetFriendlyName()
                select item;

            var types = typesQuery.ToImmutableList();
            var typePicker = new DropDown
            {
                ItemTextBinding = Binding.Property<Type?, string>(t => Format(t)),
                DataStore = types
            };

            var index = types.FindIndex(c => c.FullName == initialSettings?.Path);
            if (index >= 0)
                typePicker.SelectedIndex = index;

            var properties = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Padding = 5,
                Spacing = 5
            };

            DataContextChanged += delegate
            {
                properties.Items.Clear();

                if (DataContext is not PluginSettings settings || typePicker.SelectedValue is not Type type)
                    return;

                foreach (var control in controlBuilder.Generate(settings, type))
                    properties.Items.Add(control);
            };

            DataContext = initialSettings;

            typePicker.SelectedValueChanged += delegate
            {
                if (typePicker.SelectedValue is Type type)
                    DataContext = new PluginSettings(type);
                else
                    DataContext = null;
            };

            var editorLayout = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Spacing = 5,
                Items =
                {
                    typePicker,
                    new StackLayoutItem(properties, true)
                }
            };

            Content = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Padding = 5,
                Spacing = 5,
                Items =
                {
                    new StackLayoutItem(editorLayout, true),
                    new StackLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 5,
                        Items =
                        {
                            new StackLayoutItem(null, true),
                            new Button(Clear)
                            {
                                Text = "Clear"
                            },
                            new Button(Apply)
                            {
                                Text = "Apply"
                            }
                        }
                    }
                }
            };
        }

        private void Clear(object? sender, EventArgs e)
        {
            Close(new DialogModel<PluginSettings>(DialogResult.Ok));
        }

        private void Apply(object? sender, EventArgs e)
        {
            Close(new DialogModel<PluginSettings>(DialogResult.Ok, DataContext as PluginSettings));
        }

        private static string Format(Type? type)
        {
            return type?.GetFriendlyName() ?? type?.GetFullyQualifiedName() ?? "None";
        }
    }
}
