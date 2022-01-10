using System;

namespace OpenTabletDriver.Plugin.Attributes
{
    /// <summary>
    /// Marks a static function to be executed from the client application.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ActionAttribute : Attribute
    {
        public ActionAttribute(string groupName, string displayText)
        {
            GroupName = groupName;
            DisplayText = displayText;
        }

        public string GroupName { set; get; }
        public string DisplayText { set; get; }
    }
}
