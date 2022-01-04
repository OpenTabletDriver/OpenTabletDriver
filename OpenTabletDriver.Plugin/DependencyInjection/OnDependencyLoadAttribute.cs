using System;

namespace OpenTabletDriver.Plugin.DependencyInjection
{
    /// <summary>
    /// Marks a method to fire when all dependencies have been injected successfully.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class OnDependencyLoadAttribute : Attribute
    {
    }
}
