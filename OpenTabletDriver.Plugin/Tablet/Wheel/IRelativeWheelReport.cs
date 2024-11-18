namespace OpenTabletDriver.Plugin.Tablet.Wheel
{
    /// <summary>
    /// An auxiliary report containing states of a wheel/ring input.
    /// </summary>
    public interface IRelativeWheelReport : IDeviceReport
    {
        /// <summary>
        /// The relative wheel movement from its previous state, or null to indicate an absence of touch.
        /// </summary>
        int WheelDelta { get; set; }
    }
}
