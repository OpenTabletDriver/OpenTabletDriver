namespace OpenTabletDriver.Plugin.Tablet
{
    public interface IAbsoluteSingleAnalogReport : IDeviceReport
    {
        /// <summary>
        /// The position reading of an input, or null to indicate no reading.
        /// </summary>
        uint? Position { get; set; }
    }
}
