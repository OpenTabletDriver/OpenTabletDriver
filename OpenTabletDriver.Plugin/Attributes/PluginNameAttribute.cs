using System;

namespace OpenTabletDriver.Plugin.Attributes
{
    /// <summary>
    /// Applies a friendly name of a plugin class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class PluginNameAttribute : Attribute
    {
        public PluginNameAttribute(string name)
        {
            Name = name;
        }

        public readonly string Name;
    }
}
