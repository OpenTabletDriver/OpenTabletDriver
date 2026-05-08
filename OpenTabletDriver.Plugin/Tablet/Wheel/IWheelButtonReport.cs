namespace OpenTabletDriver.Plugin.Tablet.Wheel
{
    /// <summary>
    /// A report containing aux keys related to all wheels on a tablet
    /// <para/>
    /// This is effectively an <see cref="IAuxReport"/> but 2-dimensional (to account for multiple wheels), and separated
    /// out to better indicate its separation in CLI/GUI.
    /// </summary>
    /// <remarks>
    /// Down the line this interface may be merged with <see cref="IAuxReport"/> if wheel buttons are separated differently
    /// </remarks>
    public interface IWheelButtonReport : IDeviceReport
    {
        /// <summary>
        /// The buttons per wheel, e.g. <c>[[True], [False]]</c> for multi-wheel.
        /// Or <c>[[True]]</c> for single wheel
        /// </summary>
        bool[][] WheelButtons { set; get; }
    }
}
