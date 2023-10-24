namespace OpenTabletDriver.Tools.LibinputQuirks
{
    class TabletAttributes
    {
        // libinput quirks docs:
        // https://wayland.freedesktop.org/libinput/doc/latest/device-quirks.html

        // Toggle whether smoothing should be enabled or not.
        // This is enabled by default in libinput, but disabled by default for us.
        public bool EnableSmoothing;

        // The threshold for which the tip activates "tip-down" event.
        // Equivalent to the 'N' variable of 'AttrPressureRange=N:M' in the quirks file
        public uint? TipDownPressurePermille;

        // The threshold for which the tip activates "tip-up" event.
        // A tip-up event can only happen after a tip-down event.
        // Equivalent to the 'M' variable of 'AttrPressureRange=N:M' in the quirks file
        public uint? TipUpPressurePermille;

        public TabletAttributes(uint? tipDownPressurePermille, uint? tipUpPressurePermille, bool enableSmoothing)
        {
            TipDownPressurePermille = tipDownPressurePermille;
            TipUpPressurePermille = tipUpPressurePermille;
            EnableSmoothing = enableSmoothing;
        }
    }
}
