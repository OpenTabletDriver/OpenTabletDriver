using System.Linq.Expressions;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Output;
using OpenTabletDriver.UX.Components;

namespace OpenTabletDriver.UX.Controls.Editors
{
    public abstract class AreaEditor : DesktopPanel
    {
        protected static Control CreateUnitBox(string text, Control control, string unit)
        {
            return new GroupBox
            {
                Style = "labeled",
                Content = new StackLayout
                {
                    Orientation = Orientation.Horizontal,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Padding = new Padding(5, 0),
                    Spacing = 10,
                    Items =
                    {
                        text,
                        new StackLayoutItem(control, true),
                        unit
                    }
                }
            };
        }

        protected NumericMaskedTextBox<float> TextBoxFor(Expression<Func<AbsoluteOutputMode, float>> expression)
        {
            var memberExpression = (MemberExpression) expression.Body;
            var member = memberExpression.Member;

            var control = new NumericMaskedTextBox<float>
            {
                ID = member.Name
            };

            var binding = new DelegateBinding<float>(
                () => (DataContext as Profile)?.OutputMode.GetChild(expression) ?? 0,
                v => (DataContext as Profile)?.OutputMode.SetChild(expression, v),
                h => DataContextChanged += h,
                h => DataContextChanged -= h
            );

            control.ValueBinding.Bind(binding);

            return control;
        }
    }
}
