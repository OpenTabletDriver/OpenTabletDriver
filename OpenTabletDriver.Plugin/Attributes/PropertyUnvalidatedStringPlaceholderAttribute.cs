using System;
using JetBrains.Annotations;

namespace OpenTabletDriver.Plugin.Attributes
{
    /// <summary>
    /// Marks a non-<see cref="PropertyValidatedAttribute"/> <see cref="PropertyAttribute"/> with an underlying
    /// string type to have a placeholder text
    /// </summary>
    /// <remarks>Will likely have no effect if the annotated type is not a <see langword="string"/> without <see cref="PropertyValidatedAttribute"/></remarks>
    [PublicAPI, AttributeUsage(AttributeTargets.Property)]
    public class PropertyUnvalidatedStringPlaceholderAttribute(string placeholder) : Attribute
    {
        /// <summary>
        /// Placeholder form text for non-validated string types
        /// </summary>
        /// <remarks>Will likely throw <see cref="NotSupportedException"/> on lookup if used with non-validated string types</remarks>
        public string Placeholder { get; } = placeholder;
    }
}
