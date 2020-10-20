using System;

namespace OpenTabletDriver.Plugin.Attributes
{
    public enum InputRestriction
    {
        Number,
        Custom
    }

    public delegate bool Restrictor(string input);

    [AttributeUsage(AttributeTargets.Property)]
    public class InputRestrictionAttribute : ModifierAttribute
    {
        public InputRestrictionAttribute(InputRestriction restriction, Restrictor customRestrictor)
        {
            Restriction = restriction;
            CustomRestrictor = customRestrictor;
        }

        public InputRestriction Restriction { private set; get; }
        public Restrictor CustomRestrictor { private set; get; }
    }
}