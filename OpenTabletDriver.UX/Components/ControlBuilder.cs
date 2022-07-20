using System.Collections.Immutable;
using System.Numerics;
using System.Reflection;
using Eto.Forms;
using OpenTabletDriver.Attributes;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Reflection;

namespace OpenTabletDriver.UX.Components
{
    public class ControlBuilder : IControlBuilder
    {
        private readonly IServiceProvider _serviceProvider;

        public ControlBuilder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public T Build<T>(params object[] additionalDependencies) where T : Control
        {
            return _serviceProvider.CreateInstance<T>(additionalDependencies);
        }

        public IEnumerable<Control> Generate(PluginSettings? settings, Type type)
        {
            return from property in type.GetRuntimeProperties()
                let settingAttr = property.GetCustomAttribute<SettingAttribute>()
                where settingAttr != null
                let control = Generate(settings ?? new PluginSettings(type), property, settingAttr)
                where control != null
                select control;
        }

        private Control? Generate(PluginSettings settings, PropertyInfo property, SettingAttribute settingAttr)
        {
            var binding = new DelegateBinding<PluginSetting>(
                () => settings[property.Name],
                s => settings[property.Name] = s
            );

            var control = ControlForProperty(property, binding);

            if (control is null or CheckBox or LabeledGroup || control.Properties.Get<bool>("ControlOnly"))
                return control;

            return new LabeledGroup(settingAttr.DisplayName, control);
        }

        private static readonly IReadOnlyDictionary<Type, Func<PropertyInfo, DirectBinding<PluginSetting>, Control>> GenericControls
            = new Dictionary<Type, Func<PropertyInfo, DirectBinding<PluginSetting>, Control>>
        {
            { typeof(string), GetTextBox },
            { typeof(bool), GetCheckBox },
            { typeof(sbyte), GetNumericEditor<sbyte> },
            { typeof(byte), GetNumericEditor<byte> },
            { typeof(short), GetNumericEditor<short> },
            { typeof(ushort), GetNumericEditor<ushort> },
            { typeof(int), GetNumericEditor<int> },
            { typeof(uint), GetNumericEditor<uint> },
            { typeof(long), GetNumericEditor<long> },
            { typeof(ulong), GetNumericEditor<ulong> },
            { typeof(float), GetNumericEditor<float> },
            { typeof(double), GetNumericEditor<double> },
            { typeof(DateTime), GetMaskedEditor<DateTime> },
            { typeof(TimeSpan), GetTimeSpanEditor },
            { typeof(Vector2), GetVectorEditor }
        };

        private Control? ControlForProperty(PropertyInfo property, DirectBinding<PluginSetting> binding)
        {
            if (GetAttributeControl(property, binding) is Control control)
                return control;

            return GenericControls.GetValueOrDefault(property.PropertyType)?.Invoke(property, binding);
        }

        private Control? GetAttributeControl(PropertyInfo property, DirectBinding<PluginSetting> binding)
        {
            if (property.GetCustomAttribute<MemberValidatedAttribute>() is MemberValidatedAttribute memberValidated)
            {
                if (memberValidated.GetValue(_serviceProvider, property) is IEnumerable<object> enumerable)
                {
                    var dropDown = new DropDown
                    {
                        DataStore = enumerable.ToImmutableList(),
                        ID = property.Name
                    };
                    dropDown.SelectedValueBinding.Bind(binding.ValueSetting<object>());
                    return dropDown;
                }
            }

            return null;
        }

        private static Control GetTextBox(PropertyInfo property, DirectBinding<PluginSetting> binding)
        {
            var control = new TextBox
            {
                ID = property.Name
            };

            control.TextBinding.Bind(binding.ValueSetting<string>());
            return control;
        }

        private static Control GetNumericEditor<T>(PropertyInfo property, DirectBinding<PluginSetting> binding)
        {
            return GetMaskedEditor<NumericMaskedTextBox<T>, T>(property, binding);
        }

        private static Control GetMaskedEditor<T>(PropertyInfo property, DirectBinding<PluginSetting> binding)
        {
            return GetMaskedEditor<MaskedTextBox<T>, T>(property, binding);
        }

        private static Control GetMaskedEditor<TControl, T>(PropertyInfo property, DirectBinding<PluginSetting> binding)
            where TControl : MaskedTextBox<T>, new()
        {
            var control = new TControl
            {
                ID = property.Name
            };

            control.ValueBinding.Bind(binding.ValueSetting<T>());

            if (property.GetCustomAttribute<UnitAttribute>() is UnitAttribute attr)
            {
                var name = property.GetCustomAttribute<SettingAttribute>()!.DisplayName;
                return new LabeledGroup(name, control, attr.Unit);
            }

            return control;
        }

        private static Control GetCheckBox(PropertyInfo property, DirectBinding<PluginSetting> binding)
        {
            var attr = property.GetCustomAttribute<SettingAttribute>()!;
            var control = new CheckBox
            {
                Text = attr.DisplayName,
                ToolTip = attr.Description,
                ID = property.Name
            };

            control.CheckedBinding.Convert(c => c ?? false, v => v)
                .Bind(binding.ValueSetting<bool>());

            return control;
        }

        private static Control GetTimeSpanEditor(PropertyInfo property, DirectBinding<PluginSetting> binding)
        {
            var ms = new NumericMaskedTextBox<int>();

            var timeSpanBinding = binding.ValueSetting<TimeSpan>();

            ms.ValueBinding.Bind(timeSpanBinding.Convert(
                c => c.Milliseconds,
                v => TimeSpan.FromMilliseconds(v)
            ));

            var text = property.GetCustomAttribute<SettingAttribute>()!.DisplayName;
            return new LabeledGroup(text, ms, "ms");
        }

        private static Control GetVectorEditor(PropertyInfo property, DirectBinding<PluginSetting> binding)
        {
            var xComponent = new NumericMaskedTextBox<float>();
            var yComponent = new NumericMaskedTextBox<float>();

            var vectorBinding = binding.ValueSetting<Vector2>();

            xComponent.ValueBinding.Convert(
                v =>
                {
                    var value = binding.DataValue.GetValue<Vector2>();
                    value.X = v;
                    return value;
                },
                f => f.X
            ).Bind(vectorBinding);

            yComponent.ValueBinding.Convert(
                v =>
                {
                    var value = binding.DataValue.GetValue<Vector2>();
                    value.Y = v;
                    return value;
                },
                f => f.Y
            ).Bind(vectorBinding);

            var prefix = property.GetCustomAttribute<SettingAttribute>()!.DisplayName;

            var layout = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5,
                Items =
                {
                    new StackLayoutItem(new LabeledGroup($"{prefix} X", xComponent), true),
                    new StackLayoutItem(new LabeledGroup($"{prefix} Y", yComponent), true)
                },
                Properties =
                {
                    { "ControlOnly", true }
                }
            };

            if (property.GetCustomAttribute<UnitAttribute>() is UnitAttribute attr)
            {
                foreach (var group in layout.Items.Where(c => c.Control is LabeledGroup))
                {
                    var groupLayout = ((LabeledGroup) group.Control).FindChild<StackLayout>();
                    groupLayout.Items.Add(attr.Unit);
                }
            }

            return layout;
        }
    }
}
