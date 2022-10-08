using System;
using JetBrains.Annotations;

namespace OpenTabletDriver.Attributes.UI
{
    /// <summary>
    /// Designates a linked setting source.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    [PublicAPI]
    public class LinkedSettingSourceAttribute : Attribute
    {
        public static readonly Type[] SupportedTypes =
        {
            typeof(bool)
        };
    }
}
