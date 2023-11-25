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
        public RangeSettingAttribute(string displayName, float min, float max, float step) : base(displayName)
        {
            Min = min;
            Max = max;
            Step = step;
        }

        public float Min { get; }
        public float Max { get; }
        public float Step { get; }
    }
}
