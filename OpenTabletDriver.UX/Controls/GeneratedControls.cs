using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eto.Forms;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.UX.Controls.Generic;
using OpenTabletDriver.UX.Controls.Generic.Text;

namespace OpenTabletDriver.UX.Controls
{
    public static class GeneratedControls
    {
        private static readonly Dictionary<Type, Func<PropertyInfo, DirectBinding<PluginSetting>, Control>> genericControls = new()
        {
            { typeof(sbyte), GetNumericMaskedTextBox<sbyte> },
            { typeof(byte), GetNumericMaskedTextBox<byte> },
            { typeof(short), GetNumericMaskedTextBox<short> },
            { typeof(ushort), GetNumericMaskedTextBox<ushort> },
            { typeof(int), GetNumericMaskedTextBox<int> },
            { typeof(uint), GetNumericMaskedTextBox<uint> },
            { typeof(long), GetNumericMaskedTextBox<long> },
            { typeof(ulong), GetNumericMaskedTextBox<ulong> },
            { typeof(double), GetMaskedTextBox<DoubleNumberBox, double> },
            { typeof(DateTime), GetMaskedTextBox<DateTime> },
            { typeof(TimeSpan), GetMaskedTextBox<TimeSpan> }
        };

        public static Control GetControlForProperty(PluginSettingStore store, PropertyInfo property)
        {
            var attr = property.GetCustomAttribute<PropertyAttribute>();

            var settingBinding = new DelegateBinding<PluginSetting>(
                () => store[property],
                (v) => store[property] = v
            );

            var control = GetControlForSetting(property, settingBinding);

            // Apply all visual modifier attributes
            control = property.GetCustomAttributes<ModifierAttribute>().Aggregate(control, ApplyModifierAttribute);

            control.Width = 400;
            return new Group(attr?.DisplayName ?? property.Name, control, Orientation.Horizontal, false);
        }

        private static Control GetControlForSetting(PropertyInfo property, DirectBinding<PluginSetting> binding)
        {
            Control? rv = null;
            if (property.PropertyType == typeof(string))
            {
                if (property.GetCustomAttribute<PropertyValidatedAttribute>() is PropertyValidatedAttribute validateAttr)
                {
                    var comboBox = new DropDown<string>
                    {
                        DataStore = validateAttr.GetValue<IEnumerable<string>>(property),
                    };
                    comboBox.SelectedItemBinding.Bind(binding.Convert<string?>(property));
                    rv = comboBox;
                }
                else
                {
                    var textBox = new TextBox();
                    textBox.TextBinding.Bind(binding.Convert<string>(property));
                    rv = textBox;
                }
            }
            else if (property.PropertyType == typeof(bool))
            {
                string description = property.Name;
                if (property.GetCustomAttribute<BooleanPropertyAttribute>() is BooleanPropertyAttribute attribute)
                    description = attribute.Description;

                var checkbox = new CheckBox
                {
                    Text = description
                };
                checkbox.CheckedBinding.Cast<bool>().Bind(
                    binding.Convert(
                        s => s.GetValueOrDefault<bool>(property),
                        v => new PluginSetting(property, v)
                    )
                );
                rv = checkbox;
            }
            else if (property.PropertyType == typeof(float))
            {
                var tb = GetMaskedTextBox<FloatNumberBox, float>(property, binding);

                if (property.GetCustomAttribute<SliderPropertyAttribute>() is SliderPropertyAttribute sliderAttr)
                {
                    // TODO: replace with slider when possible (https://github.com/picoe/Eto/issues/1772)
                    tb.ToolTip = $"Minimum: {sliderAttr.Min}, Maximum: {sliderAttr.Max}";
                    tb.PlaceholderText = $"{sliderAttr.DefaultValue}";

                    if (!binding.DataValue.HasValue)
                        binding.DataValue.SetValue(sliderAttr.DefaultValue);
                }
                rv = tb;
            }
            else if (genericControls.TryGetValue(property.PropertyType, out var getGenericTextBox))
            {
                rv = getGenericTextBox(property, binding);
            }

            if (rv == null)
                throw new NotSupportedException($"'{property.PropertyType}' is not supported for generated controls.");

            rv.UpdateBindings(); // avoid empty/default values becoming null in settings
            return rv;
        }

        private static Control ApplyModifierAttribute(Control control, ModifierAttribute attribute)
        {
            switch (attribute)
            {
                case ToolTipAttribute toolTipAttr:
                {
                    control.ToolTip = toolTipAttr.ToolTip;
                    return control;
                }
                // This might cause issues if this is done before another attribute.
                case UnitAttribute unitAttr:
                {
                    var label = new Label { Text = unitAttr.Unit };
                    var layout = new StackLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 5,
                        Items =
                        {
                            new StackLayoutItem(control, true),
                            new StackLayoutItem(label, VerticalAlignment.Center)
                        }
                    };
                    return layout;
                }
                default:
                    return control;
            }
        }

        private static NumericMaskedTextBox<T> GetNumericMaskedTextBox<T>(PropertyInfo property, DirectBinding<PluginSetting> binding)
        {
            return GetMaskedTextBox<NumericMaskedTextBox<T>, T>(property, binding);
        }

        private static MaskedTextBox<T> GetMaskedTextBox<T>(PropertyInfo property, DirectBinding<PluginSetting> binding)
        {
            return GetMaskedTextBox<MaskedTextBox<T>, T>(property, binding);
        }

        private static TControl GetMaskedTextBox<TControl, T>(PropertyInfo property, DirectBinding<PluginSetting> binding) where TControl : MaskedTextBox<T>, new()
        {
            var textBox = new TControl();
            textBox.ValueBinding.Bind(binding.Convert<T>(property)!);
            return textBox;
        }

        private static DirectBinding<T?> Convert<T>(this DirectBinding<PluginSetting> binding, PropertyInfo property)
        {
            return binding.Convert(
                s => s.GetValueOrDefault<T>(property),
                v => new PluginSetting(property, v)
            );
        }
    }
}
