using System;
using JetBrains.Annotations;

namespace OpenTabletDriver.Plugin.Attributes
{
    /// <summary>
    /// Creates a slider for a property value between <see cref="Min"/> and <see cref="Max"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    [MeansImplicitUse(ImplicitUseKindFlags.Access | ImplicitUseKindFlags.Assign)]
    public class SliderPropertyAttribute(string displayName, float min, float max, float defaultValue = 0f)
        : PropertyAttribute(displayName)
    {
        public float Min { set; get; } = min;
        public float Max { set; get; } = max;
        public float DefaultValue { set; get; } = defaultValue;
    }
}
