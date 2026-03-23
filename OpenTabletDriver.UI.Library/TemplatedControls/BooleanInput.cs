using Avalonia;
using Avalonia.Data;

namespace OpenTabletDriver.UI.TemplatedControls;

public class BooleanInput : DescribedInput
{
    private bool _value;

    public static readonly DirectProperty<BooleanInput, bool> ValueProperty =
        AvaloniaProperty.RegisterDirect<BooleanInput, bool>(
            nameof(Value),
            o => o.Value,
            (o, v) => o.Value = v,
            defaultBindingMode: BindingMode.TwoWay
        );

    public bool Value
    {
        get => _value;
        set => SetAndRaise(ValueProperty, ref _value, value);
    }
}
