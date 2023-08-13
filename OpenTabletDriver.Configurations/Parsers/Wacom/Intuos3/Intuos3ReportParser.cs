using System;
using System.Numerics;
using OpenTabletDriver.Configurations.Parsers.Wacom.IntuosV1;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.Intuos3
{
    public class Intuos3ReportParser : IReportParser<IDeviceReport>
    {
        public virtual IDeviceReport Parse(byte[] data)
        {
            return data[0] switch
            {
                0x02 => GetToolReport(data),
                0x10 => new IntuosV1TabletReport(data, ref _prevPressure, ref _prevTilt, ref _prevPenButtons),
                0x03 => new IntuosV1AuxReport(data),
                0x0C => new Intuos3AuxReport(data),
                _ => new DeviceReport(data)
            };
        }

        private IDeviceReport GetToolReport(byte[] data)
        {
            if (data[1] == 0xEA || data[1] == 0xAA)
                return new IntuosV1RotationReport(data, ref _prevPressure, ref _prevTilt, ref _prevPenButtons);
            if ((data[1] & 0xF0) == 0xE0 || (data[1] & 0xF0) == 0xA0)
                return new IntuosV1TabletReport(data, ref _prevPressure, ref _prevTilt, ref _prevPenButtons);
            if ((data[1] & 0xF0) == 0xF0 || (data[1] & 0xF0) == 0xB0)
                return new Intuos3MouseReport(data);

            return new DeviceReport(data);
        }

        private uint _prevPressure;
        private Vector2 _prevTilt;
        private bool[] _prevPenButtons = Array.Empty<bool>();
    }
}
