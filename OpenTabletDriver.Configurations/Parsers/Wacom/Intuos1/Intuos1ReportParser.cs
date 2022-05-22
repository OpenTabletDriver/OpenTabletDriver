using System;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.Intuos1
{
    public class Intuos1ReportParser : IReportParser<IDeviceReport>
    {
        public virtual IDeviceReport Parse(byte[] report)
        {
            return report[0] switch
            {
                0x02 => GetToolReport(report),
                0x10 => GetToolReport(report),
                _ => new DeviceReport(report)
            };
        }

        private IDeviceReport GetToolReport(byte[] report)
        {
            if (report[0] == 0x10 && report[1] == 0x20)
                return new DeviceReport(report);
            if (report[1] == 0x80)
                return new OutOfRangeReport(report);
            if (report[1].IsBitSet(5))
                return new Intuos1TabletReport(report, ref ToolTypeByte);
            if (report[1] == 0xC2)
                return new Intuos1ToolReport(report, ref ToolTypeByte);
                

            return new DeviceReport(report);
        }

        private byte ToolTypeByte;
    }
}
