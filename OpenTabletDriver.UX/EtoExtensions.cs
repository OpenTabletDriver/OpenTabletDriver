using System.Linq.Expressions;
using System.Reflection;
using Eto.Forms;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.UX.Dialogs;

namespace OpenTabletDriver.UX
{
    public static class EtoExtensions
    {
        private static ExceptionDialog? _exceptionDialog;

        /// <summary>
        /// Shows an exception with a MessageBox dialog.
        /// </summary>
        /// <param name="ex">The exception to show.</param>
        public static void Show(this Exception ex)
        {
            if (_exceptionDialog != null)
                return;

            Application.Instance.Invoke(() =>
            {
                try
                {
                    // Grab innermost exception
                    while (ex.InnerException != null)
                        ex = ex.InnerException;

                    _exceptionDialog = new ExceptionDialog(ex);
                    _exceptionDialog.ShowModal(Application.Instance.MainForm);
                    _exceptionDialog = null;
                }
                catch
                {
                    Console.WriteLine(ex);
                }
            });
        }

        /// <summary>
        /// Starts a task on the default task scheduler.
        /// </summary>
        /// <param name="task">The task to start.</param>
        public static void Run(this Task task)
        {
            _ = task.ContinueWith(t => t.Exception?.Show(), TaskContinuationOptions.OnlyOnFaulted);
        }

        /// <summary>
        /// Returns a <see cref="Func{T}"/> for a plugin setting.
        /// </summary>
        /// <param name="settings">The settings source.</param>
        /// <param name="expression">The expression pointing to the setting.</param>
        /// <typeparam name="TModel">The settings type.</typeparam>
        /// <typeparam name="T">The setting target type.</typeparam>
        public static Func<T> GetterFor<TModel, T>(
            this PluginSettings settings,
            Expression<Func<TModel, T>> expression
        )
        {
            var body = (MemberExpression) expression.Body;

            var property = (PropertyInfo) body.Member;
            var propertyName = property.Name;
            var propertyType = property.PropertyType;

            return () => (T)settings[propertyName].GetValue(propertyType)!;
        }

        /// <summary>
        /// Converts a PluginSetting binding into a specific type.
        /// </summary>
        /// <param name="binding">The binding to convert</param>
        /// <typeparam name="TValue">The target value type</typeparam>
        public static DirectBinding<TValue> ValueSetting<TValue>(this DirectBinding<PluginSetting> binding)
        {
            return binding.Convert(
                c => c.GetValue<TValue>()!,
                v => binding.DataValue.SetValue(v)
            );
        }

        /// <summary>
        /// Pretty prints <see cref="PluginSettings"/> to a human readable string.
        /// </summary>
        /// <param name="settings">The settings to format</param>
        /// <param name="factory">Plugin factory</param>
        public static string Format(this PluginSettings settings, IPluginFactory factory)
        {
            var friendlyName = factory.GetFriendlyName(settings.Path) ?? settings.Path;

            if (!settings.Settings.Any())
                return friendlyName;

            return friendlyName + ": [ " + string.Join(", ", settings.Settings) + " ]";
        }

        /// <summary>
        /// Returns a friendly name from an expression.
        /// </summary>
        /// <param name="expression">The expression pointing to the target member.</param>
        /// <typeparam name="T">The source type.</typeparam>
        /// <typeparam name="TValue">The target member type.</typeparam>
        /// <returns>A human-readable string or the member name.</returns>
        public static string GetFriendlyName<T, TValue>(this Expression<Func<T, TValue>> expression)
        {
            var memberExpression = expression.Body switch
            {
                UnaryExpression unEx => (MemberExpression) unEx.Operand,
                object m => (MemberExpression)m
            };

            var member = memberExpression.Member;
            return member.GetCustomAttribute<System.ComponentModel.DisplayNameAttribute>()?.DisplayName ?? member.Name;
        }

        /// <summary>
        /// Modifies a <see cref="PluginSetting"/> in <see cref="PluginSettings"/> with a new value based on an <paramref name="expression"/>.
        /// </summary>
        /// <param name="settings">The settings source.</param>
        /// <param name="expression">The target property.</param>
        /// <param name="value">The value to set.</param>
        /// <typeparam name="TType">The source type.</typeparam>
        /// <typeparam name="TValue">The target value type.</typeparam>
        public static void Set<TType, TValue>(this PluginSettings settings, Expression<Func<TType, TValue>> expression, TValue value)
        {
            var name = MemberNameFor(expression);
            settings[name].SetValue(value);
        }

        /// <summary>
        /// Returns <typeparamref name="TValue"/> from a <see cref="PluginSetting"/> based on <paramref name="expression"/>.
        /// </summary>
        /// <param name="settings">The settings source.</param>
        /// <param name="expression">The target property.</param>
        /// <typeparam name="TType">The source type.</typeparam>
        /// <typeparam name="TValue">The target value type.</typeparam>
        public static TValue Get<TType, TValue>(this PluginSettings settings, Expression<Func<TType, TValue>> expression)
        {
            var name = MemberNameFor(expression);
            return settings[name].GetValue<TValue>()!;
        }

        /// <summary>
        /// Returns <typeparamref name="TValue"/> from a <see cref="PluginSetting"/> child property based on <paramref name="expression"/>.
        /// </summary>
        /// <param name="settings">The settings source.</param>
        /// <param name="expression">The target child property.</param>
        /// <typeparam name="TType">The source type.</typeparam>
        /// <typeparam name="TValue">The target value type.</typeparam>
        public static TValue GetChild<TType, TValue>(this PluginSettings settings, Expression<Func<TType, TValue>> expression)
        {
            var propertyExpression = (MemberExpression) expression.Body;
            var property = (PropertyInfo) propertyExpression.Member;

            var parentExpression = (MemberExpression) propertyExpression.Expression!;
            var parentName = parentExpression.Member.Name;
            var parentObj = settings[parentName].GetValue(parentExpression.Type);

            return (TValue) property.GetValue(parentObj)!;
        }

        /// <summary>
        /// Modifies a <see cref="PluginSetting"/>'s child property in <see cref="PluginSettings"/> with a new value based on an <paramref name="expression"/>.
        /// </summary>
        /// <param name="settings">The settings source.</param>
        /// <param name="expression">The target child property.</param>
        /// <param name="value">The value to set within the child property.</param>
        /// <typeparam name="TType">The source type.</typeparam>
        /// <typeparam name="TValue">The target value type.</typeparam>
        public static void SetChild<TType, TValue>(this PluginSettings settings, Expression<Func<TType, TValue>> expression, TValue value)
        {
            var propertyExpression = (MemberExpression) expression.Body;
            var property = (PropertyInfo) propertyExpression.Member;

            var parentExpression = (MemberExpression) propertyExpression.Expression!;
            var parentName = parentExpression.Member.Name;
            var parentObj = settings[parentName].GetValue(parentExpression.Type);

            property.SetValue(parentObj, value);
            settings[parentName].SetValue(parentObj);
        }

        /// <summary>
        /// Returns the name of a target member.
        /// </summary>
        /// <param name="expression">An expression to the target member.</param>
        /// <typeparam name="TType">The source type.</typeparam>
        /// <typeparam name="TValue">The target value type.</typeparam>
        private static string MemberNameFor<TType, TValue>(Expression<Func<TType, TValue>> expression)
        {
            var body = (MemberExpression) expression.Body;

            var member = body.Member;
            var memberName = member.Name;
            return memberName;
        }
    }
}
