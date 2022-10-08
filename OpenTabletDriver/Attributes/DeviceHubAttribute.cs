using System;
using JetBrains.Annotations;

namespace OpenTabletDriver.Attributes
{
    /// <summary>
    /// Marks a class as a device hub.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    [PublicAPI]
    public class DeviceHubAttribute : Attribute
    {
    }
}
