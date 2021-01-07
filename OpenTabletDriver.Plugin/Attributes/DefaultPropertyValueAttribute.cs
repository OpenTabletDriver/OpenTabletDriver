using System;

namespace OpenTabletDriver.Plugin.Attributes
{
    public class DefaultPropertyValueAttribute : Attribute
    {
        public object Value { get; }
        public DefaultPropertyValueAttribute(object Value)
        {
            this.Value = Value;
        }
    }
}