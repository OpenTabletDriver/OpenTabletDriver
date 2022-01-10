using System;

namespace OpenTabletDriver.Plugin.Attributes
{
    /// <summary>
    /// Creates a slider for a property value between <see cref="Min"/> and <see cref="Max"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SliderPropertyAttribute : PropertyAttribute
    {
        public SliderPropertyAttribute(string displayName, float min, float max, float defaultValue = 0f) : base(displayName)
        {
            Min = min;
            Max = max;
            DefaultValue = defaultValue;
        }

        public float Min { set; get; }
        public float Max { set; get; }
        public float DefaultValue { set; get; }
    }
}
