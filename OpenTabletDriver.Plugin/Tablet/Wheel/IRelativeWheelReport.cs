namespace OpenTabletDriver.Plugin.Tablet.Wheel
{
    /// <summary>
    /// An auxiliary report containing states of a wheel/ring input.
    /// </summary>
    public interface IRelativeWheelReport : IDeviceReport
    {
        /// <summary>
        /// The relative wheel movement from its previous state, or 0 to indicate an absence of touch.
        /// </summary>
        int WheelDelta { get; set; }
    }
}
