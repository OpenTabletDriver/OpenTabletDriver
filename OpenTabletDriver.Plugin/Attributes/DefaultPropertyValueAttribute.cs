using System;

namespace OpenTabletDriver.Plugin.Attributes
{
    public class DefaultPropertyValueAttribute : Attribute
    {
        public DefaultPropertyValueAttribute(object Value)
        {
            this.Value = Value;
        }

        public object Value { get; }
    }
}