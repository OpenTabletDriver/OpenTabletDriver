using System;
using System.Linq;
using System.Reflection;

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
            return member.MemberType switch
            {
                MemberTypes.Property => (T)sourceType.GetProperty(MemberName).GetValue(null),
                MemberTypes.Field => (T)sourceType.GetField(MemberName).GetValue(null),
                MemberTypes.Method => (T)sourceType.GetMethod(MemberName).Invoke(null, null),
                _ => default(T)
            };
        }
    }
}