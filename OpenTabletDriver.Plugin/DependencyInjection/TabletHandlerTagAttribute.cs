using System;

namespace OpenTabletDriver.Plugin.DependencyInjection
{
    /// <summary>
    /// Marks a property or field as a receiver of TabletHandler
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class TabletHandlerTagAttribute : Attribute
    {
    }
}