using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using Eto.Forms;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.UX.Components;
using OpenTabletDriver.UX.Controls.Editors;
using OpenTabletDriver.UX.Utilities;
using Container = Eto.Forms.Container;

namespace OpenTabletDriver.UX.Controls
{
    public abstract class BindingPanel : DesktopPanel
    {
        private readonly IControlBuilder _controlBuilder;

        protected BindingPanel(IControlBuilder controlBuilder)
        {
            _controlBuilder = controlBuilder;
        }

        protected IEnumerable<Control> ButtonsFor(Expression<Func<TabletHandler, PluginSettingsCollection>> expression, uint count)
        {
            // this.((TabletHandler)DataContext).<expression>
            var thisExpr = Expression.Constant(this);
            var dataContextExpr = Expression.Property(thisExpr, nameof(DataContext));
            var castExpr = Expression.Convert(dataContextExpr, typeof(TabletHandler));
            var baseExpr = new ExpressionMemberAccessor().AccessMember(castExpr, expression);
            var getExpr = Expression.Lambda<Func<PluginSettingsCollection>>(baseExpr);
            var get = getExpr.Compile();

            var prefix = GetName(expression);

            // Enforces the button count to match what is expected
            get().SetLength(count);

            for (var i = 0; i < count; i++)
            {
                var index = i;
                var binding = new DelegateBinding<PluginSettings?>(
                    () => get()[index],
                    v => get()[index] = v,
                    h => get().CollectionChanged += h.Invoke,
                    h => get().CollectionChanged -= h.Invoke
                );
                var editor = _controlBuilder.Build<BindingEditor>(binding);
                yield return new LabeledGroup($"{prefix} {index + 1}", editor);
            }
        }

        /// <summary>
        /// Creates a button for a plugin settings.
        /// </summary>
        /// <param name="expression">Expression pointing to the plugin settings.</param>
        /// <param name="name">Friendly name for the button, otherwise pulled via reflection.</param>
        protected Container ButtonFor(Expression<Func<TabletHandler, PluginSettings?>> expression, string? name = null)
        {
            var binding = DataContextBinding.Cast<TabletHandler>().Child(expression);
            var editor = _controlBuilder.Build<BindingEditor>(binding);

            return new LabeledGroup(name ?? GetName(expression), editor);
        }

        /// <summary>
        /// Creates a <see cref="Slider"/> with a <see cref="NumericMaskedTextBox{T}"/> for fine tuning.
        /// </summary>
        /// <param name="expression">An expression pointing to a setting.</param>
        protected Control SliderFor(Expression<Func<TabletHandler, double>> expression)
        {
            var binding = DataContextBinding.Cast<TabletHandler>().Child(expression);

            var slider = new Slider
            {
                MinValue = 0,
                MaxValue = 100
            };

            var valueBinding = new BindableBinding<Slider, double>(
                slider,
                c => c.Value,
                (c, v) => c.Value = (int)v,
                (s, h) => s.ValueChanged += h,
                (s, h) => s.ValueChanged -= h
            );
            valueBinding.Bind(binding);

            var textBox = new NumericMaskedTextBox<double>();
            textBox.ValueBinding.Bind(binding);

            var stackLayout = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                VerticalContentAlignment = VerticalAlignment.Center,
                Spacing = 5,
                Items =
                {
                    new StackLayoutItem(slider, true),
                    textBox
                }
            };

            return new LabeledGroup(GetName(expression), stackLayout);
        }

        /// <summary>
        /// Returns a friendly name for the target member in an expression.
        /// </summary>
        /// <param name="expression">The expression pointing to a target member.</param>
        /// <typeparam name="T">The target member type.</typeparam>
        private static string GetName<T>(Expression<Func<TabletHandler, T>> expression)
        {
            var body = expression.Body switch
            {
                MemberExpression expr => expr,
                UnaryExpression uExpr => (MemberExpression) uExpr.Operand,
                _ => throw new NotSupportedException()
            };
            var member = body.Member;
            return member.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? member.Name;
        }
    }
}
