using System;

namespace OpenTabletDriver.Plugin.Attributes
{
    /// <summary>
    /// Applies a tooltip to a property on the client.
    /// </summary>
    public class ToolTipAttribute : ModifierAttribute
    {
        public ToolTipAttribute(string tooltip)
        {
            this.ToolTip = tooltip;
        }

        public string ToolTip { private set; get; }
    }
}
