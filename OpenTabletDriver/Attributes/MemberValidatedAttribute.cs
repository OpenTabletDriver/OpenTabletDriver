using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace OpenTabletDriver.Attributes
{
    /// <summary>
    /// Marks a member that is used for validating setting properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    [PublicAPI]
    public partial class MemberValidatedAttribute : Attribute
    {
        public MemberValidatedAttribute(string memberName, bool requiresInstance = false)
        {
            MemberName = memberName;
            RequiresInstance = requiresInstance;
        }

        /// <summary>
        /// The target member for validating setting properties.
        /// </summary>
        public string MemberName { get; }

        /// <summary>
        /// Whether an instance of the object is needed for validation to occur.
        /// </summary>
        public bool RequiresInstance { get; }

        public object? GetValue(IServiceProvider serviceProvider, PropertyInfo property)
        {
            var sourceType = property.ReflectedType!;
            var member = sourceType.GetMember(MemberName).First();
            var instance = RequiresInstance ? serviceProvider.GetRequiredService(sourceType) : null;

            try
            {
                var obj = member.MemberType switch
                {
                    MemberTypes.Property => sourceType.GetProperty(MemberName)!.GetValue(instance),
                    MemberTypes.Field => sourceType.GetField(MemberName)!.GetValue(instance),
                    MemberTypes.Method => sourceType.GetMethod(MemberName)!.Invoke(instance, new object[] { serviceProvider }),
                    _ => default
                };
                return obj;
            }
            catch (Exception e)
            {
                Log.Write("Plugin", $"Failed to get valid binding values for '{MemberName}'", LogLevel.Error);

                var match = NonStaticTargetRegex().Match(e.Message);

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

        [GeneratedRegex("Non-static (.*) requires a target\\.")]
        private static partial Regex NonStaticTargetRegex();
    }
}
