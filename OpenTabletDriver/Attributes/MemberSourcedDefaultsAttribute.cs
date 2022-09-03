using System;
using JetBrains.Annotations;

namespace OpenTabletDriver.Attributes
{
    /// <summary>
    /// Applies the default value to a property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    [PublicAPI]
    public class MemberSourcedDefaultsAttribute : Attribute
    {
        public MemberSourcedDefaultsAttribute(string targetMemberName)
        {
            TargetMemberName = targetMemberName;
        }

        public string TargetMemberName { get; }
    }
}
