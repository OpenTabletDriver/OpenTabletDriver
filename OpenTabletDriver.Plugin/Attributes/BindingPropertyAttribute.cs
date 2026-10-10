using System;

namespace OpenTabletDriver.Plugin.Attributes
{
    /// <summary>
    /// Marks a property as a binding, which is configured with a binding editor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class BindingPropertyAttribute : PropertyAttribute
    {
        public BindingPropertyAttribute(string displayName) : base(displayName)
        {
        }
    }
}
