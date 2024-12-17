namespace OpenTabletDriver.Plugin.Tablet
{
    public interface IRelativeSingleAnalogReport : IDeviceReport
    {
        /// <summary>
        /// The delta calculated from the last and current input, if any.
        /// </summary>
        int? Delta { get; set; }
    }
}