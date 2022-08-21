using System;
using JetBrains.Annotations;

namespace OpenTabletDriver.Attributes
{
    /// <summary>
    /// Base attribute class for attributes which provide extra information to a property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Property)]
    [PublicAPI]
    public abstract class ModifierAttribute : Attribute
    {
    }
}
