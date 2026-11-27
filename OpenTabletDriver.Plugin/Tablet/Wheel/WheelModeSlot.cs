namespace OpenTabletDriver.Plugin.Tablet.Wheel
{
    public class WheelModeSlot
    {
        public bool Enabled { get; set; }
        public string Name { get; set; }
        public WheelActionType ActionType { get; set; }
        public string PositiveKey { get; set; }
        public string NegativeKey { get; set; }
    }
}
