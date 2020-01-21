using System;

namespace TabletDriverPlugin.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
    public class PluginIgnoreAttribute : Attribute
    {
    }
}