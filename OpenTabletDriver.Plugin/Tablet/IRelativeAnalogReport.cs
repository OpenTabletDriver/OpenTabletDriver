namespace OpenTabletDriver.Plugin.Tablet
{
    public interface IRelativeAnalogReport : IDeviceReport
    {
        /// <summary>
        /// The delta in analog gadget positions. 0 equals no movement
        /// </summary>
        int[] AnalogDeltas { get; set; }
    }
}
