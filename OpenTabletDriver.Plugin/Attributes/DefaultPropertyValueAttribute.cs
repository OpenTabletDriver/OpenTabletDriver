using System;

namespace OpenTabletDriver.Plugin.Attributes
{
    /// <summary>
    /// Applies the default value to a property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DefaultPropertyValueAttribute : Attribute
    {
        public DefaultPropertyValueAttribute(object value)
        {
            this.Value = value;
        }

        public object Value { get; }
    }
}
