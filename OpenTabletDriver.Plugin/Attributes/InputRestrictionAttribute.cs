using System;

namespace OpenTabletDriver.Plugin.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class InputRestrictionAttribute : ModifierAttribute
    {
        public InputRestrictionAttribute(RestrictionType restriction)
        {
            Restriction = restriction;
        }

        public InputRestrictionAttribute(Func<string, bool> customRestrictor)
        {
            Restriction = RestrictionType.Custom;
            CustomRestrictor = customRestrictor;
        }

        public RestrictionType Restriction { private set; get; }
        public Func<string, bool> CustomRestrictor { private set; get; }
    }
}