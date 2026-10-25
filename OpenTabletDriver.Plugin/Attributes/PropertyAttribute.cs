using System;
using JetBrains.Annotations;

namespace OpenTabletDriver.Plugin.Attributes
{
    /// <summary>
    /// Marks a property to be modified and saved by a client to settings.
    /// </summary>
    [PublicAPI]
    [AttributeUsage(AttributeTargets.Property)]
    [MeansImplicitUse(ImplicitUseKindFlags.Access | ImplicitUseKindFlags.Assign)]
    public class PropertyAttribute(string displayName) : Attribute
    {
        public string DisplayName { get; } = displayName;
    }
}
