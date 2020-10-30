using System;

namespace OpenTabletDriver.Plugin.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class InputRestrictionAttribute : ModifierAttribute
    {
        public InputRestrictionAttribute(InputRestriction restriction)
        {
            Restriction = restriction;
        }

        public InputRestrictionAttribute(InputRestriction restriction, Func<string, bool> customRestrictor)
        {
            Restriction = restriction;
            CustomRestrictor = customRestrictor;
        }

        public InputRestriction Restriction { private set; get; }
        public Func<string, bool> CustomRestrictor { private set; get; }
    }
}