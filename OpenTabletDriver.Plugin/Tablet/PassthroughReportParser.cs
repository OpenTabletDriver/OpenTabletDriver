using System;

namespace OpenTabletDriver.Plugin.Tablet
{
    public class PassthroughReportParser : IReportParser
    {
        private static bool fakeParse(scoped ref DeviceReport deviceReport) => deviceReport.Continue = true;

        public bool Parse(scoped in ReadOnlySpan<byte> report, scoped ref DeviceReport deviceReport)
        {
            return fakeParse(ref deviceReport);
        }
    }
}
