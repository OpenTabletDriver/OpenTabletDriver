using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Acepen
{
    public class AcepenReportParser : IReportParser<IDeviceReport>
    {
        private const byte PEN_MODE = 0x41;
        private const byte AUX_MODE = 0x42;
        private readonly bool[] auxState = new bool[8];

        public IDeviceReport Parse(byte[] report)
        {
            return report[1] switch
            {
                PEN_MODE => (report[2] & 0xf0) switch
                {
                    0xA0 => new AcepenTabletReport(report),
                    _ => new DeviceReport(report)
                },
                AUX_MODE => new AcepenAuxReport(report, auxState),
                _ => new DeviceReport(report)
            };
        }
    }
}
