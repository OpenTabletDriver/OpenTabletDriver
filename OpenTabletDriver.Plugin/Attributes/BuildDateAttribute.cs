using System;

namespace OpenTabletDriver.Plugin.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class BuildDateAttribute : Attribute
    {
        public string BuildDate { get; }
        public BuildDateAttribute(string buildDate)
        {
            BuildDate = buildDate;
        }
    }
}
