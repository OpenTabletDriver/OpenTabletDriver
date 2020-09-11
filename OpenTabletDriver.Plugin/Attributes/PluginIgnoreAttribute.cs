using System;

namespace OpenTabletDriver.Plugin.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
    public class PluginIgnoreAttribute : Attribute
    {
    }
}