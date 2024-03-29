using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;

namespace OpenTabletDriver.UI.TemplatedControls;

[TemplatePart("PART_Background", typeof(Border))]
public abstract class DescribedInput : TemplatedControl
{
    private Border? _background;
    private ToolTip? _toolTip;

    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<DescribedInput, string?>(nameof(Label));

    public static readonly StyledProperty<object?> DescriptionProperty =
        AvaloniaProperty.Register<DescribedInput, object?>(nameof(Description));

    public static readonly StyledProperty<int> TimeToShowDescInMsProperty =
        AvaloniaProperty.Register<DescribedInput, int>(nameof(TimeToShowDescInMs), 400);

    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public object? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public int TimeToShowDescInMs
    {
        get => GetValue(TimeToShowDescInMsProperty);
        set => SetValue(TimeToShowDescInMsProperty, value);
    }

    static DescribedInput()
    {
        TimeToShowDescInMsProperty.Changed.AddClassHandler<DescribedInput>((sender, args) => sender.OnTimeToShowDescInMsChanged(args));
        DescriptionProperty.Changed.AddClassHandler<DescribedInput>((sender, args) => sender.OnDescriptionChanged(args));
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _background = e.NameScope.Get<Border>("PART_Background");
        _background.GotFocus += (sender, args) =>
        {
            if (_toolTip is not null)
                ToolTip.SetIsOpen(_background, false);
        };

        ToolTip.SetTip(_background, _toolTip);
        ToolTip.SetShowDelay(_background, TimeToShowDescInMs);

        base.OnApplyTemplate(e);
    }

    private void OnTimeToShowDescInMsChanged(AvaloniaPropertyChangedEventArgs args)
    {
        if (_background is not null && _toolTip is not null)
        {
            ToolTip.SetShowDelay(_background, args.GetNewValue<int>());
        }
    }

    private void OnDescriptionChanged(AvaloniaPropertyChangedEventArgs args)
    {
        _toolTip = !string.IsNullOrEmpty(args.NewValue as string)
            ? new ToolTip
            {
                Content = new Panel
                {
                    Children =
                    {
                        new ContentControl
                        {
                            [!ContentControl.ContentProperty] = this[!DescriptionProperty]
                        }
                    }
                },
            } : null;

        if (_background is not null)
        {
            ToolTip.SetTip(_background, _toolTip);
        }
    }
}
