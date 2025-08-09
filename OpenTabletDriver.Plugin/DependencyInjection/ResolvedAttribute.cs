using System;

namespace OpenTabletDriver.Plugin.DependencyInjection
{
    /// <summary>
    /// Marks a property or field to be resolved with dependency injection.
    /// It's value will be set as soon as the object is constructed.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class ResolvedAttribute : Attribute
    {
    }
}
