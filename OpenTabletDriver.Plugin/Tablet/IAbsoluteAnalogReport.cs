namespace OpenTabletDriver.Plugin.Tablet
{
    public interface IAbsoluteAnalogReport : IDeviceReport
    {
        /// <summary>
        /// The absolute position readings of an input, or null to indicate no relevant reading for this report.
        /// For example, a touch-based absolute wheel may report no touch or no state change.
        /// </summary>
        uint?[] AnalogPositions { get; set; }
    }
}
