using System;
using System.Numerics;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.IntuosV1
{
    public class IntuosV1ReportParser : IReportParser
    {
        public virtual bool Parse(scoped in ReadOnlySpan<byte> report, scoped ref DeviceReport deviceReport)
        {
            return deviceReport.Continue = report[0] switch
            {
                0x02 or 0x10 => GetToolReport(in report, ref deviceReport),
                0x03 => IntuosV1AuxReport.Parse(in report, ref deviceReport),
                _ => false,
            };
        }

        protected bool GetToolReport(scoped in ReadOnlySpan<byte> report, ref DeviceReport deviceReport)
        {
            if (report[0] == 0x10 && report[1] == 0x20)
                return false;
            if (report[1] == 0x80)
            {
                deviceReport.OutOfRange = true;
                return true;
            }
            if (report[1].IsBitSet(1) && report[1].IsBitSet(3))
                return IntuosV1RotationReport.Parse(in report, ref deviceReport, ref _prevPressure, ref _prevTilt, ref _prevRotation, ref _prevPenButtons);
            if (report[1].IsBitSet(5))
                return IntuosV1TabletReport.Parse(in report, ref deviceReport, ref _prevPressure, ref _prevTilt, ref _prevRotation, ref _prevPenButtons);
            if (report[1] == 0xC2)
                return IntuosV1ToolReport.Parse(in report, ref deviceReport);

            return false;
        }

        private uint _prevPressure;
        private Vector2 _prevTilt;
        private int _prevRotation;
        private bool[] _prevPenButtons = Array.Empty<bool>();
    }
}
