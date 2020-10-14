using System;

namespace OpenTabletDriver.Plugin.Attributes
{
    public class ToolTipAttribute : ModifierAttribute
    {
        public ToolTipAttribute(string tooltip)
        {
            this.ToolTip = tooltip;
        }

        public string ToolTip { private set; get; }
    }
}