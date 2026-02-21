using System;
using JetBrains.Annotations;

namespace OpenTabletDriver.Plugin.Attributes
{
    /// <summary>
    /// Marks a property to be modified and saved by a client to settings.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    [MeansImplicitUse(ImplicitUseKindFlags.Access | ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.Itself)]
    public class PropertyAttribute(string displayName) : Attribute
    {
        public string DisplayName { set; get; } = displayName;
    }
}
