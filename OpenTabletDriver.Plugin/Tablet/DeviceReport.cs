namespace OpenTabletDriver.Plugin.Tablet
{
    public struct DeviceReport : IDeviceReport
    {
        public DeviceReport(byte[] report)
        {
            Raw = report;
        }

        public byte[] Raw { set; get; }
    }
}
