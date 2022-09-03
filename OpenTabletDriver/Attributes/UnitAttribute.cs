using System;
using JetBrains.Annotations;

namespace OpenTabletDriver.Attributes
{
    /// <summary>
    /// Applies a unit suffix to a property on the client.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    [PublicAPI]
    public class UnitAttribute : ModifierAttribute
    {
        public UnitAttribute(string unit)
        {
            Unit = unit;
        }

        public string Unit { get; }
    }
}
