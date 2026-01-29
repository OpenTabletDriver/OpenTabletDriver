namespace OpenTabletDriver.Plugin.Tablet.Wheel
{
    /// <summary>
    /// An auxiliary report containing states of multiple relative wheel/ring/strip inputs.
    /// Each wheel is identified by an index and reports its delta independently.
    /// </summary>
    public interface IMultiRelativeWheelReport : IDeviceReport
    {
        /// <summary>
        /// The index of the wheel that generated this report (0-based).
        /// </summary>
        int WheelIndex { get; }

        /// <summary>
        /// The delta calculated from the last and current input for this specific wheel.
        /// </summary>
        int? Delta { get; set; }
    }
}
