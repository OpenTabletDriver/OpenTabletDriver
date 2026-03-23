using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;

namespace OpenTabletDriver.UI.TemplatedControls;

[TemplatePart("PART_Input", typeof(TextBox))]
public class DoubleInput : DescribedInput
{
    private double? _value;
    private string _text = "0";
    private string _previousText = "0";
    private bool _updating; // prevent feedback loop

    public static readonly DirectProperty<DoubleInput, double?> ValueProperty =
        AvaloniaProperty.RegisterDirect<DoubleInput, double?>(
            nameof(Value),
            o => o.Value,
            (o, v) => o.Value = v,
            defaultBindingMode: BindingMode.TwoWay
        );

    public static readonly DirectProperty<DoubleInput, string> TextProperty =
        AvaloniaProperty.RegisterDirect<DoubleInput, string>(
            nameof(Text),
            o => o.Text,
            (o, v) => o.Text = v,
            unsetValue: "0",
            defaultBindingMode: BindingMode.TwoWay
        );

    public static readonly StyledProperty<string?> UnitProperty =
        AvaloniaProperty.Register<DoubleInput, string?>(nameof(Unit));

    public static readonly StyledProperty<int> PrecisionProperty =
        AvaloniaProperty.Register<DoubleInput, int>(nameof(Precision), 2);

    public double? Value
    {
        get => _value;
        set => SetAndRaise(ValueProperty, ref _value, value);
    }

    public string Text
    {
        get => _text;
        set => SetAndRaise(TextProperty, ref _text, value);
    }

    public string? Unit
    {
        get => GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

    public int Precision
    {
        get => GetValue(PrecisionProperty);
        set => SetValue(PrecisionProperty, value);
    }

    static DoubleInput()
    {
        // these handlers update the other property
        ValueProperty.Changed.AddClassHandler<DoubleInput>((o, e) => o.OnValueChanged(e));
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        var input = e.NameScope.Get<TextBox>("PART_Input");
        input.TextChanging += OnTextChanging;

        base.OnApplyTemplate(e);
    }

    protected virtual void HandleValueChanged(AvaloniaPropertyChangedEventArgs e)
    {
        Text = e.NewValue is double newValue
            ? newValue.ToString(CultureInfo.InvariantCulture)
            : "0";
    }

    private void OnValueChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (_updating)
            return;

        _updating = true;
        _previousText = e.NewValue?.ToString() ?? string.Empty;
        HandleValueChanged(e);
        _updating = false;
    }

    private void OnTextChanging(object? sender, TextChangingEventArgs e)
    {
        var input = (TextBox)sender!;
        e.Handled = true;
        if (_updating)
            return;

        _updating = true;
        if (string.IsNullOrEmpty(input!.Text))
        {
            // special case for empty string
            // not doing this results to having 0 in textbox when input is cleared
            _previousText = string.Empty;
            Value = 0;
        }
        else if (StringUtility.TryParseDouble(input.Text, out double result))
        {
            var roundedResult = Math.Round(result, Precision);
            if (roundedResult != result)
            {
                var resultStr = !input.Text.EndsWith(".")
                    ? result.ToString(CultureInfo.InvariantCulture)
                    : input.Text;
                input.Text = resultStr;
            }

            _previousText = input.Text;
            Value = result; // allow feedback here for stuff like range checking in derived classes
        }
        else
        {
            // if parsing fails, revert text to previous value
            input.Text = _previousText;
        }

        _updating = false;
    }
}
