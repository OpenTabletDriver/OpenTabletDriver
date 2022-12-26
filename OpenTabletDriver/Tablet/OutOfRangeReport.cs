using JetBrains.Annotations;

namespace OpenTabletDriver.Tablet
{
    /// <summary>
    /// A device report designating that no useful data is parsed.
    /// Typically designates that no buttons are pressed and no tool is in range.
    /// </summary>
    [PublicAPI]
    public struct OutOfRangeReport : IDeviceReport
    {
        public OutOfRangeReport(byte[] report)
        {
            Raw = report;
        }

        public byte[] Raw { set; get; }
    }
}
