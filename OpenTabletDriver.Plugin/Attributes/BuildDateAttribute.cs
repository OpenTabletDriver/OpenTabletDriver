using System;

namespace OpenTabletDriver.Plugin.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly)]
    internal class BuildDateAttribute : Attribute
    {
        public string BuildDate { get; }
        public BuildDateAttribute(string buildDate)
        {
            BuildDate = buildDate;
        }
    }
}