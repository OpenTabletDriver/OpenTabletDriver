using System;

namespace OpenTabletDriver.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class LegacyDeviceHubAttribute : Attribute
    {
        public string Protocol { get; }

        public LegacyDeviceHubAttribute(string protocol)
        {
            Protocol = protocol;
        }
    }
}
