using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using OpenTabletDriver.UI.TemplatedControls;
using OpenTabletDriver.UI.ViewModels.Plugin;

namespace OpenTabletDriver.UI.DataTemplates;

public class PluginSettingControlBuilder : IDataTemplate
{
    public bool Match(object? data)
    {
        return data is PluginSettingViewModel;
    }

    public Control Build(object? data)
    {
        return data switch
        {
            BoolViewModel boolViewModel => BuildControl(boolViewModel),
            IntegerViewModel integerViewModel => BuildControl(integerViewModel),
            NumberViewModel numberViewModel => BuildControl(numberViewModel),
            StringViewModel stringViewModel => BuildControl(stringViewModel),
            null => new TextBlock { Text = "eh???" },
            _ => new TextBlock { Text = "Unknown setting type" }
        };
    }

    // TODO: migrate to compiled binding, probably convert to xaml usercontrol just to do that

    private static Control WithCommonBinds(PluginSettingViewModel vm, DescribedInput control)
    {
        var labelBinding = new Binding
        {
            Source = vm,
            Path = nameof(PluginSettingViewModel.FriendlyName)
        };

        var descBinding = new Binding
        {
            Source = vm,
            Path = nameof(PluginSettingViewModel.Description)
        };

        var toolTipBinding = new Binding
        {
            Source = vm,
            Path = nameof(PluginSettingViewModel.ToolTip)
        };

        control[!DescribedInput.LabelProperty] = labelBinding;
        control.Description = new StackPanel
        {
            Children =
            {
                new TextBlock
                {
                    [!TextBlock.TextProperty] = descBinding
                },
                new TextBlock
                {
                    [!TextBlock.TextProperty] = toolTipBinding
                }
            }
        };

        return control;
    }

    private static Control BuildControl(BoolViewModel viewModel)
    {
        var valueBinding = new Binding
        {
            Source = viewModel,
            Path = nameof(BoolViewModel.Value),
        };

        var control = new BooleanInput
        {
            [!BooleanInput.ValueProperty] = valueBinding,
        };

        return WithCommonBinds(viewModel, control);
    }

    private static Control BuildControl(IntegerViewModel viewModel)
    {
        var valueBinding = new Binding
        {
            Source = viewModel,
            Path = nameof(IntegerViewModel.Value),
        };

        DescribedInput input;

        if (viewModel.Slider)
        {
            var minBinding = new Binding
            {
                Source = viewModel,
                Path = nameof(IntegerViewModel.Min),
            };

            var maxBinding = new Binding
            {
                Source = viewModel,
                Path = nameof(IntegerViewModel.Max),
            };

            input = new RangedInput
            {
                [!DoubleInput.ValueProperty] = valueBinding,
                [!RangedInput.MinimumProperty] = minBinding,
                [!RangedInput.MaximumProperty] = maxBinding,
            };
        }
        else
        {
            input = new DoubleInput
            {
                [!DoubleInput.ValueProperty] = valueBinding,
                Precision = 0
            };
        }

        return WithCommonBinds(viewModel, input);
    }

    private static Control BuildControl(NumberViewModel viewModel)
    {
        var valueBinding = new Binding
        {
            Source = viewModel,
            Path = nameof(NumberViewModel.Value),
        };

        DescribedInput input;

        if (viewModel.Slider)
        {
            var minBinding = new Binding
            {
                Source = viewModel,
                Path = nameof(NumberViewModel.Min),
            };

            var maxBinding = new Binding
            {
                Source = viewModel,
                Path = nameof(NumberViewModel.Max),
            };

            input = new RangedInput
            {
                [!DoubleInput.ValueProperty] = valueBinding,
                [!RangedInput.MinimumProperty] = minBinding,
                [!RangedInput.MaximumProperty] = maxBinding,
            };
        }
        else
        {
            input = new DoubleInput
            {
                [!DoubleInput.ValueProperty] = valueBinding,
                Precision = 4
            };
        }

        return WithCommonBinds(viewModel, input);
    }

    private static Control BuildControl(StringViewModel viewModel)
    {
        var valueBinding = new Binding
        {
            Source = viewModel,
            Path = nameof(StringViewModel.Value),
        };

        DescribedInput input;

        if (viewModel.Choices?.Length > 0)
        {
            var choicesBinding = new Binding
            {
                Source = viewModel,
                Path = nameof(StringViewModel.Choices),
            };

            input = new EnumInput
            {
                [!EnumInput.SelectedValueProperty] = valueBinding,
                [!EnumInput.ValuesProperty] = choicesBinding,
            };
        }
        else
        {
            input = new StringInput
            {
                [!StringInput.ValueProperty] = valueBinding,
            };
        }

        return WithCommonBinds(viewModel, input);
    }
}
