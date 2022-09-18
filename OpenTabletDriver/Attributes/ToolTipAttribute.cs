using System;
using JetBrains.Annotations;

namespace OpenTabletDriver.Attributes
{
    /// <summary>
    /// Applies a tooltip to a property on the client.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    [PublicAPI]
    public class ToolTipAttribute : ModifierAttribute
    {
        public ToolTipAttribute(string tooltip)
        {
            ToolTip = tooltip;
        }

        public string ToolTip { get; }
    }
}
