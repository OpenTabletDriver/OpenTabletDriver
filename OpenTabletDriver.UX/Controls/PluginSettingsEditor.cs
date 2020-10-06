using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Native;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Reflection;

namespace OpenTabletDriver.UX.Controls
{
    public class PluginSettingsEditor<T> : Panel
    {
        public PluginSettingsEditor(string friendlyName = null)
        {
            this.FriendlyName = friendlyName;

            var content = new Splitter();
            content.Orientation = Orientation.Horizontal;
            content.Panel1MinimumSize = 200;
            content.Panel1 = new Scrollable { Content = _pluginList };
            content.Panel2 = new Scrollable { Content = _settingControls };

            Plugins = new List<PluginReference>();
            _pluginList.SelectedIndexChanged += (sender, e) =>
            {
                if (_pluginList.SelectedIndex >= 0 && _pluginList.SelectedIndex <= Plugins.Count)
                    SelectedPlugin = Plugins[_pluginList.SelectedIndex];
            };

            foreach (var type in PluginManager.GetChildTypes<T>())
            {
                var pluginRef = new PluginReference(type);
                if (type != typeof(T) && !Plugins.Contains(pluginRef))
                {
                    Plugins.Add(pluginRef);
                }
            }

            _pluginList.Items.Clear();
            foreach (var plugin in Plugins)
                _pluginList.Items.Add(string.IsNullOrWhiteSpace(plugin.Name) ? plugin.Path : plugin.Name);

            if (Plugins.Count == 0)
            {
                this.Content = new StackLayout
                {
                    Items =
                    {
                        new StackLayoutItem(null, true),
                        new StackLayoutItem($"No plugins containing {(string.IsNullOrWhiteSpace(this.FriendlyName) ? typeof(T).Name : $"{this.FriendlyName.ToLower()}s")} are installed.")
                        {
                            HorizontalAlignment = HorizontalAlignment.Center
                        },
                        new StackLayoutItem
                        {
                            Control = new LinkButton
                            {
                                Text = "Plugin Repository",
                                Command = new Command(
                                    (s, e) => SystemInfo.Open(App.PluginRepositoryUrl)
                                )
                            },
                            HorizontalAlignment = HorizontalAlignment.Center
                        },
                        new StackLayoutItem(null, true)
                    }
                };
            }
            else
            {
                this.Content = content;
            }
        }

        private List<PluginReference> Plugins = new List<PluginReference>();

        public string FriendlyName { set; get; }

        private PluginReference _selectedPlugin;
        public PluginReference SelectedPlugin
        {
            set
            {
                _selectedPlugin = value;
                _settingControls.Items.Clear();
                foreach (var control in GeneratePropertyControls())
                    _settingControls.Items.Add(new StackLayoutItem(control, HorizontalAlignment.Stretch));
            }
            get => _selectedPlugin;
        }

        private ListBox _pluginList = new ListBox();
        private StackLayout _settingControls = new StackLayout
        {
            Padding = new Padding(5),
            Spacing = 5
        };

        private IEnumerable<Control> GeneratePropertyControls()
        {
            var type = SelectedPlugin.GetTypeReference<T>();

            // Used to toggle the type's state
            var enableControl = new CheckBox
            {
                Text = "Enable"
            };
            enableControl.CheckedBinding.Bind(() => GetPluginEnabled(), (state) => SetPluginEnabled(state.Value));
            yield return enableControl;

            foreach (var property in type.GetProperties())
            {
                PropertyAttribute propertyAttr = null;
                try
                {
                    propertyAttr = property.GetCustomAttribute<PropertyAttribute>(false);
                    if (propertyAttr == null)
                        continue;
                }
                catch (TypeLoadException tlex)
                {
                    Log.Write(
                        "Plugin",
                        $"Type loading failed for '{tlex.TypeName}'. The plugin '{type.Assembly.GetName().Name}' is out of date.",
                        LogLevel.Error
                    );
                    SetPluginEnabled(false);
                    break;
                }
                
                var path = type.FullName + "." + property.Name;
                var control = GetControl(
                    property,
                    propertyAttr,
                    () => App.Settings.PluginSettings.TryGetValue(path, out var val) ? val : string.Empty,
                    (value) => 
                    {
                        if (!App.Settings.PluginSettings.TryAdd(path, value))
                            App.Settings.PluginSettings[path] = value;
                    }
                );

                foreach (var modAttr in property.GetCustomAttributes<ModifierAttribute>(false))
                    control = ApplyModifierAttributes(control, modAttr);

                yield return new GroupBox
                {
                    Content = control,
                    Text = string.IsNullOrWhiteSpace(propertyAttr.DisplayName) ? property.Name : propertyAttr.DisplayName,
                    Padding = App.GroupBoxPadding
                };
            }
            
            var staticMethods = from method in type.GetMethods()
                where method.IsStatic
                select method;
            
            foreach (var method in staticMethods)
            {
                var attributes = from attr in method.GetCustomAttributes(false)
                    where attr is ActionAttribute
                    select attr;

                foreach (ActionAttribute attr in attributes)
                {
                    var control = new Button((sender, e) => method.Invoke(null, null))
                    {
                        Text = attr.DisplayText,
                    };

                    yield return new GroupBox
                    {
                        Content = control,
                        Text = attr.GroupName,
                        Padding = App.GroupBoxPadding
                    };
                }
            }
        }

        private Control GetControl(PropertyInfo property, PropertyAttribute attr, Func<string> getValue, Action<string> setValue)
        {
            switch (attr)
            {
                case BooleanPropertyAttribute boolAttr:
                {
                    var checkBox = new CheckBox
                    {
                        Text = boolAttr.Description
                    };
                    checkBox.CheckedBinding.Convert(
                        (b) => b.Value.ToString(),
                        (string str) => bool.TryParse(str, out var val) ? val : false)
                        .Bind(getValue, setValue);
                    return checkBox;
                }
                case SliderPropertyAttribute sliderAttr:
                {
                    var tb = new TextBox
                    {
                        ToolTip = $"Minimum: {sliderAttr.Min}, Maximum: {sliderAttr.Max}",
                        PlaceholderText = $"{sliderAttr.DefaultValue}"
                    };
                    tb.TextBinding.Bind(getValue, setValue);
                    return tb;
                }
                default:
                {
                    var tb = new TextBox();
                    tb.TextBinding.Bind(getValue, setValue);
                    return tb;
                }
            }
        }

        private Control ApplyModifierAttributes(Control control, ModifierAttribute attribute)
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

        public Action<bool> SetPluginEnabled;
        public Func<bool> GetPluginEnabled;
    }
}