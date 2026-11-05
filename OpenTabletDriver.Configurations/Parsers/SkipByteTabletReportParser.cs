using System;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers
{
    public class SkipByteTabletReportParser : TabletReportParser
    {
        public override bool Parse(scoped in ReadOnlySpan<byte> report, scoped ref DeviceReport deviceReport)
        {
            return base.Parse(report[1..], ref deviceReport);
        }
    }
}
