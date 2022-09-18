using System;
using JetBrains.Annotations;

namespace OpenTabletDriver.Attributes
{
    /// <summary>
    /// Creates a range setting for a property value between <see cref="Min"/> and <see cref="Max"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    [PublicAPI]
    public class RangeSettingAttribute : SettingAttribute
    {
        public RangeSettingAttribute(string displayName, float min, float max, float defaultValue = 0f) : base(displayName)
        {
            Min = min;
            Max = max;
            DefaultValue = defaultValue;
        }

        public float Min { get; }
        public float Max { get; }
        public float DefaultValue { get; }
    }
}
