using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Plugin.Tablet.Wheel
{
    public static class WheelDefaults
    {
        public static WheelModeSlot[] CreateDefault()
        {
            return new WheelModeSlot[]
            {
                new WheelModeSlot
                {
                    Enabled = true,
                    Name = "Scroll",
                    ActionType = WheelActionType.Scroll
                },
                new WheelModeSlot
                {
                    Enabled = true,
                    Name = "Canvas Zoom",
                    ActionType = WheelActionType.CanvasZoom,
                    PositiveKey = "Ctrl+Plus",
                    NegativeKey = "Ctrl+Minus"
                },
                new WheelModeSlot
                {
                    Enabled = true,
                    Name = "Brush Size",
                    ActionType = WheelActionType.BrushSize,
                    PositiveKey = "]",
                    NegativeKey = "["
                },
                new WheelModeSlot { Enabled = false, Name = "Mode 4" },
                new WheelModeSlot { Enabled = false, Name = "Mode 5" },
                new WheelModeSlot { Enabled = false, Name = "Mode 6" }
            };
        }

        public static WheelModeSlot[] LoadFromConfig(TabletReference tablet)
        {
            return tablet.Properties.WheelModes ?? CreateDefault();
        }
    }
}
