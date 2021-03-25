using System;

namespace OpenTabletDriver.Plugin.DependencyInjection
{
    /// <summary>
    /// Marks a property to be resolved with dependency injection.
    /// It's value will be set as soon as the object is constructed.
    /// </summary>
    public class ResolvedAttribute : Attribute
    {
    }
}