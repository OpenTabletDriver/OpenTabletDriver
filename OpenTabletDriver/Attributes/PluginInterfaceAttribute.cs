using System;

namespace OpenTabletDriver.Attributes
{
    /// <summary>
    /// Marks a base class or interface as an implementable plugin interface.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
    public class PluginInterfaceAttribute : Attribute
    {
    }
}
