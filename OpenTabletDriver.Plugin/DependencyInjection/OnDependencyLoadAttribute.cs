using System;
using JetBrains.Annotations;

namespace OpenTabletDriver.Plugin.DependencyInjection
{
    /// <summary>
    /// Marks a method to fire when all dependencies have been injected successfully.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    [MeansImplicitUse(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.Itself)]
    public class OnDependencyLoadAttribute : Attribute;
}
