using System;
using JetBrains.Annotations;

namespace OpenTabletDriver.Attributes
{
    /// <summary>
    /// Applies a friendly name of a plugin class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
    [MeansImplicitUse(ImplicitUseTargetFlags.WithMembers)]
    [PublicAPI]
    public class PluginNameAttribute : Attribute
    {
        public PluginNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
