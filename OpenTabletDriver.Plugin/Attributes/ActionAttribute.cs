using System;

namespace OpenTabletDriver.Plugin.Attributes
{
    /// <summary>
    /// Marks a static function to be executed from the client application.
    /// <b>WARNING:</b> Currently unimplemented in codebase as of v0.6.7
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ActionAttribute(string groupName, string displayText) : Attribute
    {
        public string GroupName { set; get; } = groupName;
        public string DisplayText { set; get; } = displayText;
    }
}
