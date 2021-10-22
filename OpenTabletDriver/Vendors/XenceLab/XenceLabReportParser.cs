using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Tablet;
using OpenTabletDriver.Vendors.XP_Pen;

namespace OpenTabletDriver.Vendors.XenceLab
{
    public class XenceLabReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] report)
        {
            var reportByte = report[1];
            if (reportByte.IsBitSet(5))
            {
                return new XP_PenTabletReport(report);
            }

            return (reportByte & 0xf0) switch
            {
                0xf0 => new XenceLabAuxReport(report),
                _ => new DeviceReport(report)
            };
        }
    }

    public struct XenceLabAuxReport : IAuxReport
    {
        public XenceLabAuxReport(byte[] report)
        {
            Raw = report;

            var auxByte = report[2];
            AuxButtons = new[]
            {
                auxByte.IsBitSet(0),
                auxByte.IsBitSet(1),
                auxByte.IsBitSet(2),
                auxByte.IsBitSet(3),
                auxByte.IsBitSet(4),
                auxByte.IsBitSet(5),
                auxByte.IsBitSet(6),
                auxByte.IsBitSet(7)
            };
        }

        public bool[] AuxButtons { get; set; }
        public byte[] Raw { get; set; }
    }
}