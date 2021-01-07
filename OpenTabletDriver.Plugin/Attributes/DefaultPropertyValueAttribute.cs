using System;

namespace OpenTabletDriver.Plugin.Attributes
{
    public class DefaultPropertyValueAttribute : Attribute
    {
        public DefaultPropertyValueAttribute(object value)
        {
            this.Value = value;
        }

        public object Value { get; }
    }
}