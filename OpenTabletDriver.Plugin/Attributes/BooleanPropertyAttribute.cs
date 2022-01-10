using System;

namespace OpenTabletDriver.Plugin.Attributes
{
    /// <summary>
    /// Marks a property to be modified as a boolean.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class BooleanPropertyAttribute : PropertyAttribute
    {
        public BooleanPropertyAttribute(string displayName, string description) : base(displayName)
        {
            Description = description;
        }

        public string Description { set; get; }
    }
}
