using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Plugin.Tablet
{
    public struct OutOfRangeReport : IDeviceReport
    {
        public OutOfRangeReport(byte[] report)
        {
            Raw = report;
        }

        public byte[] Raw { set; get; }
    }
}
