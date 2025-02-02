using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Data;

namespace OpenTabletDriver.UI.TemplatedControls;

[TemplatePart("PART_Input", typeof(ComboBox))]
public class EnumInput : DescribedInput
{
    private string? _selectedValue;
    private IList<string>? _values;

    public static readonly DirectProperty<EnumInput, string?> SelectedValueProperty =
        AvaloniaProperty.RegisterDirect<EnumInput, string?>(
            nameof(SelectedValue),
            o => o.SelectedValue,
            (o, v) => o.SelectedValue = v,
            defaultBindingMode: BindingMode.TwoWay
        );

    public static readonly DirectProperty<EnumInput, IList<string>?> ValuesProperty =
        AvaloniaProperty.RegisterDirect<EnumInput, IList<string>?>(
            nameof(Values),
            o => o.Values,
            (o, v) => o.Values = v
        );

    public string? SelectedValue
    {
        get => _selectedValue;
        set => SetAndRaise(SelectedValueProperty, ref _selectedValue, value);
    }

    public IList<string>? Values
    {
        get => _values;
        set => SetAndRaise(ValuesProperty, ref _values, value);
    }
}
