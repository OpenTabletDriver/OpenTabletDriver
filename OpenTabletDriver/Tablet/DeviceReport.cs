using JetBrains.Annotations;

namespace OpenTabletDriver.Tablet
{
    [PublicAPI]
    public struct DeviceReport : IDeviceReport
    {
        public DeviceReport(byte[] report)
        {
            Raw = report;
        }

        public byte[] Raw { set; get; }
    }
}
