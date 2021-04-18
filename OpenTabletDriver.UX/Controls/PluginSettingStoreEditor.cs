using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eto.Forms;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.UX.Controls.Generic;
using OpenTabletDriver.UX.Controls.Generic.Text;

namespace OpenTabletDriver.UX.Controls
{
    public class PluginSettingStoreEditor<TSource> : StackView
    {
        public PluginSettingStoreEditor()
        {
            base.Padding = 5;
            base.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            base.VerticalContentAlignment = VerticalAlignment.Top;
        }

        private static readonly IReadOnlyDictionary<Type, Func<PropertyInfo, PluginSetting, Control>> genericControls
            = new Dictionary<Type, Func<PropertyInfo, PluginSetting, Control>>
        {
            { typeof(sbyte), (a,b) => GetNumericMaskedTextBox<sbyte>(a,b) },
            { typeof(byte), (a,b) => GetNumericMaskedTextBox<byte>(a,b) },
            { typeof(short), (a,b) => GetNumericMaskedTextBox<short>(a,b) },
            { typeof(ushort), (a,b) => GetNumericMaskedTextBox<ushort>(a,b) },
            { typeof(int), (a,b) => GetNumericMaskedTextBox<int>(a,b) },
            { typeof(uint), (a,b) => GetNumericMaskedTextBox<uint>(a,b) },
            { typeof(long), (a,b) => GetNumericMaskedTextBox<long>(a,b) },
            { typeof(ulong), (a,b) => GetNumericMaskedTextBox<ulong>(a,b) },
            { typeof(double), (a,b) => BindNumberBox(new DoubleNumberBox(), a, b) },
            { typeof(DateTime), (a,b) => GetMaskedTextBox<DateTime>(a,b) },
            { typeof(TimeSpan), (a,b) => GetMaskedTextBox<TimeSpan>(a,b) }
        };

        public PluginSettingStore Store { protected set; get; }

        public virtual void Refresh(PluginSettingStore store)
        {
            this.Items.Clear();

            var enableButton = new CheckBox
            {
                Text = $"Enable {store.GetPluginReference().Name ?? store.Path}",
                Checked = store.Enable
            };
            enableButton.CheckedChanged += (sender, e) => store.Enable = enableButton.Checked ?? false;
            AddControl(enableButton);

            foreach (var control in GetControlsForStore(store))
            {
                this.Items.Add(control);
            }

            Store = store;
        }

        private IEnumerable<Control> GetControlsForStore(PluginSettingStore store)
        {
            var type = store.GetPluginReference().GetTypeReference<TSource>();
            return GetControlsForType(store, type);
        }

        private IEnumerable<Control> GetControlsForType(PluginSettingStore store, Type type)
        {
            var properties = from property in type.GetProperties()
                let attrs = property.GetCustomAttributes(true)
                where attrs.Any(a => a is PropertyAttribute)
                select property;

            foreach (var property in properties)
                yield return GetControlForProperty(store, property);
        }

        private Control GetControlForProperty(PluginSettingStore store, PropertyInfo property)
        {
            var attr = property.GetCustomAttribute<PropertyAttribute>();
            PluginSetting setting = store[property];

            if (setting == null)
            {
                setting = new PluginSetting(property, null);
                store.Settings.Add(setting);
            }

            var control = GetControlForSetting(property, setting);

            if (control != null)
            {
                // Apply all visual modifier attributes
                foreach (ModifierAttribute modifierAttr in property.GetCustomAttributes<ModifierAttribute>())
                    control = ApplyModifierAttribute(control, modifierAttr);

                control.Width = 400;
                return new Group(attr.DisplayName ?? property.Name, control, Orientation.Horizontal, false);
            }
            else
            {
                throw new NullReferenceException($"{nameof(control)} is null. This is likely due to {property.PropertyType.Name} being an unsupported type.");
            }
        }

        private Control GetControlForSetting(PropertyInfo property, PluginSetting setting)
        {
            if (property.PropertyType == typeof(string))
            {
                if (property.GetCustomAttribute<PropertyValidatedAttribute>() is PropertyValidatedAttribute validateAttr)
                {
                    var comboBox = new ComboBox
                    {
                        SelectedValue = GetSetting<string>(property, setting),
                        DataStore = validateAttr.GetValue<IEnumerable<string>>(property)
                    };
                    comboBox.SelectedValueChanged += (sender, e) => setting.SetValue(comboBox.SelectedValue);
                    return comboBox;
                }
                else
                {
                    var textbox = new TextBox
                    {
                        Text = GetSetting<string>(property, setting)
                    };
                    textbox.TextChanged += (sender, e) => setting.SetValue(textbox.Text);
                    return textbox;
                }
            }
            else if (property.PropertyType == typeof(bool))
            {
                string description = property.Name;
                if (property.GetCustomAttribute<BooleanPropertyAttribute>() is BooleanPropertyAttribute attribute)
                    description = attribute.Description;

                var checkbox = new CheckBox
                {
                    Text = description,
                    Checked = GetSetting<bool>(property, setting)
                };
                checkbox.CheckedChanged += (sender, e) => setting.SetValue((bool)checkbox.Checked);
                return checkbox;
            }
            else if (property.PropertyType == typeof(float))
            {
                var tb = BindNumberBox(new FloatNumberBox(), property, setting);

                if (property.GetCustomAttribute<SliderPropertyAttribute>() is SliderPropertyAttribute sliderAttr)
                {
                    // TODO: replace with slider when possible (https://github.com/picoe/Eto/issues/1772)
                    tb.ToolTip = $"Minimum: {sliderAttr.Min}, Maximum: {sliderAttr.Max}";
                    tb.PlaceholderText = $"{sliderAttr.DefaultValue}";
                    if (!setting.HasValue)
                        setting.SetValue(sliderAttr.DefaultValue);
                }
                return tb;
            }
            else if (genericControls.TryGetValue(property.PropertyType, out var getGenericTextBox))
            {
                return getGenericTextBox(property, setting);
            }
            throw new NotSupportedException($"'{property.PropertyType}' is not supported by {nameof(PluginSettingStoreEditor<TSource>)}");
        }

        private Control ApplyModifierAttribute(Control control, ModifierAttribute attribute)
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

        private static NumericMaskedTextBox<T> GetNumericMaskedTextBox<T>(PropertyInfo property, PluginSetting setting)
        {
            var tb = new NumericMaskedTextBox<T>
            {
                Value = GetSetting<T>(property, setting)
            };
            tb.ValueChanged += (sender, e) => setting.SetValue(tb.Value);
            return tb;
        }

        private static MaskedTextBox<T> GetMaskedTextBox<T>(PropertyInfo property, PluginSetting setting)
        {
            var tb = new MaskedTextBox<T>
            {
                Value = GetSetting<T>(property, setting)
            };
            tb.ValueChanged += (sender, e) => setting.SetValue(tb.Value);
            return tb;
        }

        private static MaskedTextBox<T> BindNumberBox<T>(MaskedTextBox<T> textBox, PropertyInfo property, PluginSetting setting)
        {
            textBox.Value = GetSetting<T>(property, setting);
            textBox.ValueChanged += (sender, e) => setting.SetValue(textBox.Value);
            return textBox;
        }

        private static T GetSetting<T>(PropertyInfo property, PluginSetting setting)
        {
            if (setting.HasValue)
            {
                return setting.GetValue<T>();
            }
            else
            {
                if (property.GetCustomAttribute<DefaultPropertyValueAttribute>() is DefaultPropertyValueAttribute defaults)
                {
                    try
                    {
                        setting.SetValue(defaults.Value);
                        return (T)defaults.Value;
                    }
                    catch (Exception e)
                    {
                        Log.Write("UX", $"Failed to get custom default of {property.Name}: {e.Message}");
                    }
                }
                return default;
            }
        }
    }
}
