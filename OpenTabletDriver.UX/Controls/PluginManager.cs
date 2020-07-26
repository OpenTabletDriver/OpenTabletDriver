using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;
using TabletDriverLib;
using TabletDriverLib.Plugins;
using TabletDriverPlugin.Attributes;

namespace OpenTabletDriver.UX.Controls
{
    public class PluginManager<T> : Splitter
    {
        public PluginManager()
        {
            Orientation = Orientation.Horizontal;
            Panel1MinimumSize = 200;
            Panel1 = new Scrollable { Content = _pluginList };
            Panel2 = new Scrollable { Content = _settingControls };

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
        }

        private List<PluginReference> Plugins = new List<PluginReference>();

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

            var enableControl = new CheckBox
            {
                Text = "Enable"
            };
            enableControl.CheckedBinding.Bind(() => GetPluginEnabled.Invoke(), (obj) => SetPluginEnabled.Invoke(this, obj.Value));
            yield return enableControl;
            
            foreach (var property in type.GetProperties())
            {
                var attributes = from attr in property.GetCustomAttributes(false)
                    where attr is PropertyAttribute
                    select attr;

                foreach (PropertyAttribute attr in attributes)
                {
                    var path = type.FullName + "." + property.Name;
                    var control = GetControl(property, attr,
                        () => App.Settings.PluginSettings.TryGetValue(path, out var val) ? val : string.Empty,
                        (value) => 
                        {
                            if (!App.Settings.PluginSettings.TryAdd(path, value))
                                App.Settings.PluginSettings[path] = value;
                        });

                    yield return new GroupBox
                    {
                        Content = control,
                        Text = string.IsNullOrWhiteSpace(attr.DisplayName) ? property.Name : attr.DisplayName,
                        Padding = App.GroupBoxPadding
                    };
                }
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
            if (attr is UnitPropertyAttribute unitAttr)
            {
                var tb = new TextBox();
                tb.TextBinding.Bind(getValue, setValue);
                var label = new Label { Text = unitAttr.Unit };

                var layout = new StackLayout
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 5,
                    Items =
                    {
                        new StackLayoutItem(tb, true),
                        new StackLayoutItem(label, VerticalAlignment.Center)
                    }
                };                
                return layout;
            }
            else if (attr is BooleanPropertyAttribute boolAttr)
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
            else
            {
                var tb = new TextBox();
                tb.TextBinding.Bind(getValue, setValue);
                return tb;
            }
        }

        public event EventHandler<bool> SetPluginEnabled;
        public Func<bool> GetPluginEnabled;
    }
}