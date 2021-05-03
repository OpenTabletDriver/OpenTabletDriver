using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eto.Forms;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.UX.Controls.Generic;
using OpenTabletDriver.UX.Controls.Generic.Text;

namespace OpenTabletDriver.UX.Windows.Configurations.Controls.Specifications
{
    public class PenSpecificationsEditor : Panel
    {
        public PenSpecificationsEditor()
        {
            buttonSpecifications = new ButtonSpecificationsEditor();

            this.Content = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Spacing = 5,
                Padding = 5,
                Items =
                {
                    new StackLayoutItem
                    {
                        Control = enable = new CheckBox
                        {
                            Text = "Enable"
                        }
                    },
                    new Group
                    {
                        Text = "Max Pressure",
                        Orientation = Orientation.Horizontal,
                        Content = maxPressure = new UnsignedIntegerNumberBox()
                    },
                    new Group
                    {
                        Text = "Active Report ID",
                        Orientation = Orientation.Horizontal,
                        Content = activeReportId = new DetectionRangeBox()
                    },
                    buttonSpecifications
                }
            };

            enable.CheckedBinding.Cast<bool>().Bind(
                PenSpecificationsBinding.Convert(
                    c => c != null,
                    v => v ? new PenSpecifications() : null
                )
            );
            enable.CheckedBinding.Bind(maxPressure, c => c.Enabled);
            enable.CheckedBinding.Bind(activeReportId, c => c.Enabled);
            enable.CheckedBinding.Bind(buttonSpecifications, c => c.Enabled);
            
            maxPressure.ValueBinding.Bind(PenSpecificationsBinding.Child(c => c.MaxPressure));
            activeReportId.ValueBinding.Bind(PenSpecificationsBinding.Child(c => c.ActiveReportID));
            buttonSpecifications.ButtonSpecificationsBinding.Bind(PenSpecificationsBinding.Child(c => c.Buttons));
        }

        private CheckBox enable;
        private MaskedTextBox<uint> maxPressure;
        private MaskedTextBox<DetectionRange> activeReportId;
        private ButtonSpecificationsEditor buttonSpecifications;

        private PenSpecifications penSpecs;
        public PenSpecifications PenSpecifications
        {
            set
            {
                this.penSpecs = value;
                this.OnPenSpecificationsChanged();
            }
            get => this.penSpecs;
        }
        
        public event EventHandler<EventArgs> PenSpecificationsChanged;
        
        protected virtual void OnPenSpecificationsChanged() => PenSpecificationsChanged?.Invoke(this, new EventArgs());
        
        public BindableBinding<PenSpecificationsEditor, PenSpecifications> PenSpecificationsBinding
        {
            get
            {
                return new BindableBinding<PenSpecificationsEditor, PenSpecifications>(
                    this,
                    c => c.PenSpecifications,
                    (c, v) => c.PenSpecifications = v,
                    (c, h) => c.PenSpecificationsChanged += h,
                    (c, h) => c.PenSpecificationsChanged -= h
                );
            }
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

                public IEnumerable<int> EditPositions => Enumerable.Range(0, builder.Length);

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