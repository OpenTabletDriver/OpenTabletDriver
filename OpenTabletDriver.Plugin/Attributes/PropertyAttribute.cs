using System;

namespace OpenTabletDriver.Plugin.Attributes
{
    /// <summary>
    /// Marks a property to be modified and saved by a client to settings.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyAttribute : Attribute
    {
        public PropertyAttribute(string displayName)
        {
            DisplayName = displayName;
        }

        public string DisplayName { set; get; }
    }
}
