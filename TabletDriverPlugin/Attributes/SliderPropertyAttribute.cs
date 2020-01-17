using System;

namespace TabletDriverPlugin.Attributes
{
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