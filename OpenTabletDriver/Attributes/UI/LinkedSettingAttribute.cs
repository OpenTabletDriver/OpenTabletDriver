using System;
using JetBrains.Annotations;

namespace OpenTabletDriver.Attributes.UI
{
    /// <summary>
    /// Designates linking between settings.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    [PublicAPI]
    public class LinkedSettingAttribute : Attribute
    {
        public LinkedSettingAttribute(string targetMemberName, string settingMemberName)
        {
            TargetMemberName = targetMemberName;
            SettingMemberName = settingMemberName;
        }

        /// <summary>
        /// The target setting that is linked.
        /// </summary>
        public string TargetMemberName { get; }

        /// <summary>
        /// The setting determining whether linking will occur.
        /// </summary>
        public string SettingMemberName { get; }

        public static readonly Type[] SupportedTypes =
        {
            typeof(Area),
            typeof(AngledArea)
        };
    }
}
