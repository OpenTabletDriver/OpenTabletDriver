using System;
using JetBrains.Annotations;

namespace OpenTabletDriver.Plugin.DependencyInjection
{
    /// <summary>
    /// Marks a property or field to be resolved with dependency injection.
    /// It's value will be set as soon as the object is constructed.
    /// </summary>
    // disable silly Rider warning for 'Inherited = false' not being a valid option on properties and fields only
    // ReSharper disable once RedundantAttributeUsageProperty
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false)]
    [MeansImplicitUse(ImplicitUseKindFlags.Assign)]
    public class ResolvedAttribute : Attribute
    {
    }
}
