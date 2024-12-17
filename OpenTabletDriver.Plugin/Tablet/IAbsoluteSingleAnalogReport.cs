namespace OpenTabletDriver.Plugin.Tablet
{
    public interface ISingleAbsoluteAnalogReport : IDeviceReport
    {
        /// <summary>
        /// The position reading of an input, or null to indicate no reading.
        /// </summary>
        uint? Position { get; }
    }
}