using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Output;
using OpenTabletDriver.UX.Components;

namespace OpenTabletDriver.UX.Controls.Editors
{
    public class AbsoluteAreaEditor : DesktopPanel
    {
        private readonly IControlBuilder _controlBuilder;
        private readonly IPluginFactory _pluginFactory;
        private readonly StackLayout _advancedSettings;

        // TODO: Fix aspect ratio locking stopping SetDisplay from scaling
        public AbsoluteAreaEditor(IControlBuilder controlBuilder, IPluginFactory pluginFactory)
        {
            _controlBuilder = controlBuilder;
            _pluginFactory = pluginFactory;

            _advancedSettings = new StackLayout();

            Content = new StackLayout
            {
                Orientation = Orientation.Vertical,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Spacing = 5,
                Items =
                {
                    new StackLayoutItem(controlBuilder.Build<OutputAreaEditor>(), true),
                    new StackLayoutItem(controlBuilder.Build<InputAreaEditor>(), true),
                    new Expander
                    {
                        ID = "Advanced",
                        Header = new Panel
                        {
                            Padding = new Padding(0, 5),
                            Content = "Advanced"
                        },
                        Content = new Scrollable
                        {
                            Padding = 5,
                            Content = _advancedSettings
                        }
                    }
                }
            };
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);

            if (DataContext is not Profile profile)
                return;

            var settings = profile.OutputMode;
            var type = _pluginFactory.GetPluginType(settings.Path);

            _advancedSettings.Items.Clear();
            foreach (var control in _controlBuilder.Generate(settings, type!))
            {
                switch (control)
                {
                    // Disallow area editors in advanced settings
                    case null or AreaEditor:
                        continue;
                    case CheckBox { ID: nameof(AbsoluteOutputMode.LockAspectRatio) } lockAspectRatio:
                    {
                        var output = FindChild<OutputAreaEditor>();
                        var outputWidth = output.FindChild<NumericMaskedTextBox<float>>(nameof(Area.Width));
                        var outputHeight = output.FindChild<NumericMaskedTextBox<float>>(nameof(Area.Height));

                        var input = FindChild<InputAreaEditor>();
                        var inputWidth = input.FindChild<NumericMaskedTextBox<float>>(nameof(AngledArea.Width));
                        var inputHeight = input.FindChild<NumericMaskedTextBox<float>>(nameof(AngledArea.Height));

                        _lockingAspectRatio = lockAspectRatio.Checked ?? false;
                        lockAspectRatio.CheckedChanged += (_, _) =>
                        {
                            _lockingAspectRatio = lockAspectRatio.Checked ?? false;
                            if (_lockingAspectRatio)
                                EnforceAspectRatio(inputWidth, inputHeight, outputWidth, outputHeight);
                        };

                        HookAspectRatio(inputWidth, inputHeight, outputWidth, outputHeight);
                        HookAspectRatio(inputHeight, inputWidth, outputHeight, outputWidth);
                        HookAspectRatio(outputWidth, outputHeight, inputWidth, inputHeight);
                        HookAspectRatio(outputHeight, outputWidth, inputHeight, inputWidth);

                        break;
                    }
                    case CheckBox { ID: nameof(AbsoluteOutputMode.LockToBounds) } lockToBounds:
                    {
                        var output = FindChild<OutputAreaEditor>();
                        var outputDisplay = output.FindChild<OutputAreaDisplay>();
                        var outputWidth = output.FindChild<NumericMaskedTextBox<float>>(nameof(AbsoluteOutputMode.Output.Width));
                        var outputHeight = output.FindChild<NumericMaskedTextBox<float>>(nameof(AbsoluteOutputMode.Output.Height));
                        var outputX = output.FindChild<NumericMaskedTextBox<float>>(nameof(AbsoluteOutputMode.Output.XPosition));
                        var outputY = output.FindChild<NumericMaskedTextBox<float>>(nameof(AbsoluteOutputMode.Output.YPosition));

                        var input = FindChild<InputAreaEditor>();
                        var inputDisplay = input.FindChild<InputAreaDisplay>();
                        var inputX = input.FindChild<NumericMaskedTextBox<float>>(nameof(AbsoluteOutputMode.Input.XPosition));
                        var inputY = input.FindChild<NumericMaskedTextBox<float>>(nameof(AbsoluteOutputMode.Input.YPosition));

                        _lockingToBounds = lockToBounds.Checked ?? false;
                        lockToBounds.CheckedChanged += (_, _) =>
                        {
                            _lockingToBounds = lockToBounds.Checked ?? false;
                            if (_lockingToBounds)
                            {
                                EnforcePositionToBounds(outputX, outputWidth.Value, outputDisplay.FullBackground.Width);
                                EnforcePositionToBounds(outputY, outputHeight.Value, outputDisplay.FullBackground.Height);
                                EnforceAngledAreaToBounds(inputX, inputX, inputY, inputDisplay, GetInputArea);
                                EnforceAngledAreaToBounds(inputY, inputX, inputY, inputDisplay, GetInputArea);
                            }
                        };

                        HookPositionToBounds(outputX, outputWidth, () => outputDisplay.FullBackground.Width);
                        HookPositionToBounds(outputY, outputHeight, () => outputDisplay.FullBackground.Height);
                        LockAngledAreaToBounds(inputX, inputX, inputY, inputDisplay, GetInputArea);
                        LockAngledAreaToBounds(inputY, inputX, inputY, inputDisplay, GetInputArea);

                        break;
                    }
                }

                _advancedSettings.Items.Add(control);
            }
        }

        private bool _lockingAspectRatio;
        private bool _lockingToBounds;
        private bool _valueChanging;

        private AngledArea? GetInputArea()
        {
            return (DataContext as Profile)?.OutputMode.Get((AbsoluteOutputMode f) => f.Input);
        }

        private void HookAspectRatio(
            MaskedTextBox<float> source,
            MaskedTextBox<float> sourceOpposite,
            MaskedTextBox<float> alt,
            MaskedTextBox<float> altOpposite
        )
        {
            source.ValueChanged += delegate
            {
                if (!_lockingAspectRatio || _valueChanging)
                    return;

                _valueChanging = true;
                EnforceAspectRatio(source, sourceOpposite, alt, altOpposite);
                _valueChanging = false;
            };
        }

        private static void EnforceAspectRatio(
            MaskedTextBox<float> source,
            MaskedTextBox<float> sourceOpposite,
            MaskedTextBox<float> alt,
            MaskedTextBox<float> altOpposite
        )
        {
            sourceOpposite.Value = altOpposite.Value / alt.Value * source.Value;
        }

        private void HookPositionToBounds(
            MaskedTextBox<float> position,
            MaskedTextBox<float> axisSize,
            Func<float> getAxisBounds
        )
        {
            position.ValueChanged += delegate
            {
                if (!_lockingToBounds || _valueChanging)
                    return;

                _valueChanging = true;
                EnforcePositionToBounds(position, axisSize.Value, getAxisBounds());
                _valueChanging = false;
            };
        }

        private void LockAngledAreaToBounds(
            MaskedTextBox<float> source,
            MaskedTextBox<float> inputX,
            MaskedTextBox<float> inputY,
            AreaDisplay display,
            Func<AngledArea?> getArea
        )
        {
            source.ValueChanged += delegate
            {
                if (!_lockingToBounds || _valueChanging)
                    return;

                _valueChanging = true;
                EnforceAngledAreaToBounds(source, inputX, inputY, display, getArea);
                _valueChanging = false;
            };
        }

        private static void EnforceAngledAreaToBounds(
            MaskedTextBox<float> source,
            MaskedTextBox<float> inputX,
            MaskedTextBox<float> inputY,
            AreaDisplay display,
            Func<AngledArea?> getArea
        )
        {
            if (getArea() is AngledArea area)
            {
                var corners = area.GetCorners();
                var areaSize = RectangleF.FromSides(
                    corners.Min(t => t.X),
                    corners.Min(t => t.Y),
                    corners.Max(t => t.X),
                    corners.Max(t => t.Y)
                );
                if (source == inputX)
                    EnforcePositionToBounds(inputX, areaSize.Width, display.FullBackground.Width);
                if (source == inputY)
                    EnforcePositionToBounds(inputY, areaSize.Height, display.FullBackground.Height);
            }
        }

        private static void EnforcePositionToBounds(MaskedTextBox<float> position, float axisSize, float boundsSize)
        {
            // This stops the position value from flickering due to rounding errors
            if (axisSize > boundsSize || Math.Abs(axisSize - boundsSize) < 0.0005f)
            {
                position.Value = boundsSize / 2;
                return;
            }

            var offset = axisSize / 2;
            var normalized = position.Value - offset;
            if (normalized < 0)
                position.Value = offset;
            else if (normalized > boundsSize - axisSize)
                position.Value = boundsSize - offset;
        }
    }
}
