using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Reflection;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Controls
{
    public class PluginSettingStoreCollectionEditor<TSource> : Panel
    {
        public PluginSettingStoreCollectionEditor(
            WeakReference<PluginSettingStoreCollection> reference,
            string friendlyName = null
        )
        {
            CollectionReference = reference;

            this.baseControl.Panel1 = new Scrollable { Content = sourceSelector };
            this.baseControl.Panel2 = new Scrollable { Content = settingStoreEditor };

            sourceSelector.SelectedSourceChanged += (sender, reference) => UpdateSelectedStore(reference);

            if (sourceSelector.Plugins.Count == 0)
            {
                this.Content = new PluginSettingStoreEmptyPlaceholder(friendlyName);
            }
            else
            {
                this.Content = baseControl;
            }
        }

        public WeakReference<PluginSettingStoreCollection> CollectionReference { get; }

        public void UpdateStore(PluginSettingStoreCollection storeCollection)
        {
            CollectionReference.SetTarget(storeCollection);
            sourceSelector.Refresh();
        }

        private Splitter baseControl = new Splitter
        {
            Panel1MinimumSize = 200,
            Orientation = Orientation.Horizontal,
            BackgroundColor = SystemColors.WindowBackground
        };

        public PluginReference SelectedPlugin => sourceSelector.SelectedSource;

        private PluginSourceSelector sourceSelector = new PluginSourceSelector();
        private PluginSettingStoreEditor settingStoreEditor = new PluginSettingStoreEditor();

        private void UpdateSelectedStore(PluginReference reference)
        {
            if (CollectionReference.TryGetTarget(out PluginSettingStoreCollection storeCollection))
            {
                if (storeCollection.FirstOrDefault(store => store.Path == reference.Path) is PluginSettingStore store)
                {
                    settingStoreEditor.Refresh(store);
                }
                else
                {
                    var newStore = new PluginSettingStore(reference.GetTypeReference<TSource>(), false);
                    storeCollection.Add(newStore);
                    settingStoreEditor.Refresh(newStore);
                }
            }
        }

        private class PluginSettingStoreEmptyPlaceholder : StackView
        {
            public PluginSettingStoreEmptyPlaceholder(string friendlyName)
            {
                string pluginTypeName = string.IsNullOrWhiteSpace(friendlyName) ? typeof(TSource).Name : $"{friendlyName.ToLower()}s";
                base.Items.Add(new StackLayoutItem(null, true));
                base.Items.Add(
                    new StackLayoutItem
                    {
                        Control = new Bitmap(App.Logo.WithSize(256, 256)),
                        HorizontalAlignment = HorizontalAlignment.Center
                    }
                );
                base.Items.Add(
                    new StackLayoutItem
                    {
                        Control = $"No plugins containing {pluginTypeName} are installed.",
                        HorizontalAlignment = HorizontalAlignment.Center
                    }
                );
                base.Items.Add(
                    new StackLayoutItem
                    {
                        Control = new LinkButton
                        {
                            Text = "Plugin Repository",
                            Command = new Command(
                                (s, e) => SystemInterop.Open(App.PluginRepositoryUrl)
                            )
                        },
                        HorizontalAlignment = HorizontalAlignment.Center
                    }
                );
                base.Items.Add(new StackLayoutItem(null, true));
            }
        }

        private class PluginSourceSelector : ListBox
        {
            public PluginSourceSelector()
            {
                var items = from type in AppInfo.PluginManager.GetChildTypes<TSource>()
                    select new PluginReference(AppInfo.PluginManager, type);

                Plugins = items.ToList();

                foreach (var plugin in Plugins)
                    this.Items.Add(plugin.Name ?? plugin.Path, plugin.Path);
            }

            public IList<PluginReference> Plugins { get; }

            public PluginReference SelectedSource { protected set; get; }

            public event EventHandler<PluginReference> SelectedSourceChanged;

            public void Refresh()
            {
                var lastIndex = this.SelectedIndex;
                this.SelectedIndex = -1;
                this.SelectedIndex = lastIndex;
            }

            protected override void OnSelectedIndexChanged(EventArgs e)
            {
                base.OnSelectedIndexChanged(e);
                this.OnSelectedSourceChanged(e);
            }

            protected virtual void OnSelectedSourceChanged(EventArgs e)
            {
                if (this.SelectedIndex < 0 || this.SelectedIndex > Plugins.Count - 1)
                    return;

                SelectedSource = Plugins[this.SelectedIndex];
                SelectedSourceChanged?.Invoke(this, SelectedSource);
            }
        }

        private class PluginSettingStoreEditor : StackView
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
                { typeof(double), (a,b) => GetNumericMaskedTextBox<double>(a,b) },
                { typeof(DateTime), (a,b) => GetMaskedTextBox<DateTime>(a,b) },
                { typeof(TimeSpan), (a,b) => GetMaskedTextBox<TimeSpan>(a,b) }
            };

            public void Refresh(PluginSettingStore store)
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
                    var textbox = new TextBox
                    {
                        Text = setting.GetValue<string>()
                    };
                    textbox.TextChanged += (sender, e) => setting.SetValue(textbox.Text);
                    return textbox;
                }
                else if (property.PropertyType == typeof(bool))
                {
                    string description = property.Name;
                    if (property.GetCustomAttribute<BooleanPropertyAttribute>() is BooleanPropertyAttribute attribute)
                        description = attribute.Description;

                    var checkbox = new CheckBox
                    {
                        Text = description,
                        Checked = setting.GetValue<bool>()
                    };
                    checkbox.CheckedChanged += (sender, e) => setting.SetValue((bool)checkbox.Checked);
                    return checkbox;
                }
                else if (property.PropertyType == typeof(float))
                {
                    var tb = GetNumericMaskedTextBox<float>(property, setting);

                    if (property.GetCustomAttribute<SliderPropertyAttribute>() is SliderPropertyAttribute sliderAttr)
                    {
                        // TODO: replace with slider when possible (https://github.com/picoe/Eto/issues/1772)
                        tb.ToolTip = $"Minimum: {sliderAttr.Min}, Maximum: {sliderAttr.Max}";
                        tb.PlaceholderText = $"{sliderAttr.DefaultValue}";
                        if (setting.Value == null)
                            setting.SetValue(sliderAttr.DefaultValue);
                    }
                    return tb;
                }
                else if (genericControls.TryGetValue(property.PropertyType, out var getGenericTextBox))
                {
                    return getGenericTextBox(property, setting);
                }
                throw new NotSupportedException($"'{property.PropertyType}' is not supported by {nameof(PluginSettingStoreEditor)}");
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
                    Value = setting.GetValue<T>()
                };
                tb.ValueChanged += (sender, e) => setting.SetValue(tb.Value);
                return tb;
            }

            private static MaskedTextBox<T> GetMaskedTextBox<T>(PropertyInfo property, PluginSetting setting)
            {
                var tb = new MaskedTextBox<T>
                {
                    Value = setting.GetValue<T>()
                };
                tb.ValueChanged += (sender, e) => setting.SetValue(tb.Value);
                return tb;
            }
        }
    }
}