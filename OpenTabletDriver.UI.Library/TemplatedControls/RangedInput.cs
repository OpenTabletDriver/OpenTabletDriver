using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;

namespace OpenTabletDriver.UI.TemplatedControls;

[TemplatePart("PART_Slider", typeof(Slider))]
public class RangedInput : DoubleInput
{
    private Slider? _slider;
    public static readonly StyledProperty<double?> MinimumProperty =
        AvaloniaProperty.Register<RangedInput, double?>(nameof(Minimum));

    public static readonly StyledProperty<double?> MaximumProperty =
        AvaloniaProperty.Register<RangedInput, double?>(nameof(Maximum));

    public static readonly StyledProperty<double?> MinimumSizeForSliderProperty =
        AvaloniaProperty.Register<RangedInput, double?>(nameof(MinimumSizeForSlider), defaultValue: 300);

    public double? Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double? Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public double? MinimumSizeForSlider
    {
        get => GetValue(MinimumSizeForSliderProperty);
        set => SetValue(MinimumSizeForSliderProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _slider = e.NameScope.Get<Slider>("PART_Slider");
        base.OnApplyTemplate(e);
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);

        if (e.WidthChanged)
        {
            _slider!.IsVisible = e.NewSize.Width >= MinimumSizeForSlider;
        }
    }

    protected override void HandleValueChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is double newValue)
        {
            newValue = Math.Round(newValue, Precision);

            if (Minimum.HasValue && newValue < Minimum.Value)
            {
                newValue = Minimum.Value;
            }
            else if (Maximum.HasValue && newValue > Maximum.Value)
            {
                newValue = Maximum.Value;
            }

            var textString = newValue.ToString(CultureInfo.InvariantCulture);
            Value = newValue;
            Text = textString;
        }
        else
        {
            Text = string.Empty;
        }
    }
}
