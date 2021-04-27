using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eto.Forms;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.UX.Controls.Generic;
using OpenTabletDriver.UX.Controls.Generic.Text;

namespace OpenTabletDriver.UX.Windows.Configurations.Controls
{
    public class DigitizerIdentifierEditor : DeviceIdentifierEditor<DigitizerIdentifier>
    {
        protected override Control CreateControl(int index, DirectBinding<DigitizerIdentifier> itemBinding)
        {
            var entry = new DigitizerIdentifierEntry();
            entry.EntryBinding.Bind(itemBinding);

            return entry;
        }

        private class DigitizerIdentifierEntry : DeviceIdentifierEntry<DigitizerIdentifier>
        {
            public DigitizerIdentifierEntry()
            {
                foreach (var control in GetDigitizerControls().Reverse())
                    layout.Items.Insert(0, control);

                width.ValueBinding.BindDataContext<DigitizerIdentifier>(id => id.Width);
                height.ValueBinding.BindDataContext<DigitizerIdentifier>(id => id.Height);
                maxX.ValueBinding.BindDataContext<DigitizerIdentifier>(id => id.MaxX);
                maxY.ValueBinding.BindDataContext<DigitizerIdentifier>(id => id.MaxY);
                maxPressure.ValueBinding.BindDataContext<DigitizerIdentifier>(id => id.MaxPressure);
                activeReportId.ValueBinding.BindDataContext<DigitizerIdentifier>(id => id.ActiveReportID);
            }

            private MaskedTextBox<float> width, height, maxX, maxY;
            private MaskedTextBox<uint> maxPressure;
            private MaskedTextBox<DetectionRange> activeReportId; 

            protected IEnumerable<Control> GetDigitizerControls()
            {
                yield return new Group
                {
                    Text = "Width (mm)",
                    Content = width = new FloatNumberBox()
                };

                yield return new Group
                {
                    Text = "Height (mm)",
                    Content = height = new FloatNumberBox()
                };

                yield return new Group
                {
                    Text = "Max X (px)",
                    Content = maxX = new FloatNumberBox()
                };

                yield return new Group
                {
                    Text = "Max Y (px)",
                    Content = maxY = new FloatNumberBox()
                };

                yield return new Group
                {
                    Text = "Max Pressure",
                    Content = maxPressure = new UnsignedIntegerNumberBox()
                };

                yield return new Group
                {
                    Text = "Active Report ID",
                    Content = activeReportId = new DetectionRangeBox()
                };
            }

            private class DetectionRangeBox : MaskedTextBox<DetectionRange>
            {
                public DetectionRangeBox()
                {
                    Provider = new DetectionRangeTextProvider();
                }

                private class DetectionRangeTextProvider : IMaskedTextProvider<DetectionRange>
                {
                    private StringBuilder builder = new StringBuilder();

                    public DetectionRange Value { set; get; }

                    public string DisplayText => Text;

                    public string Text
                    {
                        set => Value = DetectionRange.Parse(value);
                        get => Value?.ToString();
                    }

                    public bool MaskCompleted => true;

                    public IEnumerable<int> EditPositions
                    {
                        get
                        {
                            for (int i = 0; i <= builder.Length; i++)
                            {
                                yield return i;
                            }
                        }
                    }

                    public bool IsEmpty => builder.Length == 0;

                    public bool Clear(ref int position, int length, bool forward)
                    {
                        return Delete(ref position, length, forward);
                    }

                    public bool Delete(ref int position, int length, bool forward)
                    {
                        if (builder.Length == 0)
                            return false;
                        if (forward)
                        {
                            length = Math.Min(length, builder.Length - position);
                            builder.Remove(position, length);
                        }
                        else if (position >= length)
                        {
                            builder.Remove(position - length, length);
                            position = Math.Max(0, position - length);
                        }
                        return true;
                    }

                    public bool Insert(char character, ref int position)
                    {
                        if (Allow(ref character, ref position))
                        {
                            builder.Insert(position, character);
                            position++;
                            return true;
                        }
                        return false;
                    }

                    public bool Replace(char character, ref int position)
                    {
                        if (Allow(ref character, ref position))
                        {
                            if (position >= builder.Length)
                                return Insert(character, ref position);
                            builder[position] = character;
                            position++;
                        }
                        return true;
                    }

                    private bool Allow(ref char character, ref int position)
                    {
                        if (position == 0)
                        {
                            return leftOperators.Contains(character);
                        }
                        else if (position == builder.Length && rightOperators.Contains(builder[builder.Length - 1]))
                        {
                            return false;
                        }
                        else
                        {
                            return char.IsDigit(character) || rightOperators.Contains(character);
                        }
                    }

                    private static readonly char[] leftOperators =
                    {
                        DetectionRange.LeftExclusiveOperator,
                        DetectionRange.LeftInclusiveOperator
                    };

                    private static readonly char[] rightOperators =
                    {
                        DetectionRange.RightExclusiveOperator,
                        DetectionRange.RightInclusiveOperator
                    };
                }
            }
        }
    }
}