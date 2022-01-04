using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace OpenTabletDriver.Plugin.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyValidatedAttribute : Attribute
    {
        public PropertyValidatedAttribute(string memberName)
        {
            MemberName = memberName;
        }

        /// <summary>
        /// The name of the member in which the property this is assigned to is allowed to have.
        /// </summary>
        /// <remarks>
        /// This member must return <see cref="IEnumerable{T}"/> statically.
        /// </remarks>
        public string MemberName { get; }

        public T GetValue<T>(PropertyInfo property)
        {
            var sourceType = property.ReflectedType;
            var member = sourceType.GetMember(MemberName).First();
            try
            {
                return member.MemberType switch
                {
                    MemberTypes.Property => (T)sourceType.GetProperty(MemberName).GetValue(null),
                    MemberTypes.Field => (T)sourceType.GetField(MemberName).GetValue(null),
                    MemberTypes.Method => (T)sourceType.GetMethod(MemberName).Invoke(null, null),
                    _ => default
                };
            }
            catch (Exception e)
            {
                Log.Write("Plugin", $"Failed to get valid binding values for '{MemberName}'", LogLevel.Error);

                var match = Regex.Match(e.Message, "Non-static (.*) requires a target\\.");

                if (e is TargetException && match.Success)
                {
                    Log.Debug("Plugin", $"Validation {match.Groups[1].Value} must be static");
                }
                else
                {
                    Log.Exception(e);
                }
            }

            return default;
        }
    }
}
