using System.Collections.Immutable;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Tablet;
using OpenTabletDriver.UX.Components;

namespace OpenTabletDriver.UX.Dialogs
{
    public class AreaConverterDialog : Dialog<AngledArea?>
    {
        public AreaConverterDialog(IServiceProvider serviceProvider, IPluginFactory pluginFactory, TabletConfiguration tablet)
        {
            Title = "Convert area...";

            var converters = pluginFactory.GetMatchingTypes(typeof(IAreaConverter)).ToImmutableList();

            var selector = new DropDown
            {
                DataStore = converters,
                ItemTextBinding = Binding.Property<Type, string>(t => t.GetFriendlyName() ?? t.GetPath())
            };

            var okButton = new Button((_, _) => Close(DataContext as AngledArea))
            {
                Text = "Ok",
                Enabled = false
            };

            var actions = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Padding = 5,
                Spacing = 5,
                Items =
                {
                    new StackLayoutItem(null, true),
                    new Button((_, _) => Close())
                    {
                        Text = "Cancel"
                    },
                    okButton
                }
            };

            var placeholder = new Panel
            {
                Padding = 10,
                Content = "No area converter is selected."
            };

            var settings = new Panel
            {
                Content = placeholder
            };

            IAreaConverter? converter = null;

            selector.SelectedIndexChanged += (_, _) =>
            {
                okButton.Enabled = selector.SelectedIndex >= 0;

                if (!okButton.Enabled)
                {
                    settings.Content = placeholder;
                    return;
                }

                var type = converters[selector.SelectedIndex];
                converter = (IAreaConverter) serviceProvider.CreateInstance(type);
                actions.Enabled = true;

                var left = CreateNumberBox();
                var right = CreateNumberBox();
                var top = CreateNumberBox();
                var bottom = CreateNumberBox();

                void ValueChanged(object? sender, EventArgs args)
                {
                    DataContext = converter.Convert(tablet, top.Value, left.Value, bottom.Value, right.Value);
                }

                left.ValueChanged += ValueChanged;
                right.ValueChanged += ValueChanged;
                top.ValueChanged += ValueChanged;
                bottom.ValueChanged += ValueChanged;

                ValueChanged(null, EventArgs.Empty);

                settings.Content = new TableLayout
                {
                    Padding = 5,
                    Spacing = new Size(5, 5),
                    Rows =
                    {
                        new TableRow
                        {
                            ScaleHeight = true,
                            Cells =
                            {
                                new TableCell(new LabeledGroup(converter.Top, null, top), true),
                                new TableCell(new LabeledGroup(converter.Bottom, null, bottom), true)
                            }
                        },
                        new TableRow
                        {
                            ScaleHeight = true,
                            Cells =
                            {
                                new TableCell(new LabeledGroup(converter.Left, null, left), true),
                                new TableCell(new LabeledGroup(converter.Right, null, right), true)
                            }
                        }
                    }
                };
            };

            selector.SelectedIndex = converters.FindIndex(c =>
            {
                converter = serviceProvider.CreateInstance(c) as IAreaConverter;
                return tablet.DigitizerIdentifiers.Any(i => (int?)converter?.Vendor == i.VendorID);
            });

            Content = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Items =
                {
                    new Panel
                    {
                        Padding = 5,
                        Content = selector
                    },
                    new StackLayoutItem(settings, true),
                    actions
                }
            };
        }

        private static NumericMaskedTextBox<float> CreateNumberBox()
        {
            return new NumericMaskedTextBox<float>
            {
                Value = 0f
            };
        }
    }
}
