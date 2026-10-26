using System;
using System.Diagnostics.CodeAnalysis;

namespace OpenTabletDriver.Plugin.Tablet
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public class TabletReportParser : IReportParser
    {
        public virtual bool Parse(scoped in ReadOnlySpan<byte> report, scoped ref DeviceReport deviceReport)
        {
            deviceReport.Parser = typeof(TabletReportParser);
            return deviceReport.Continue = TabletReport.Parse(in report, ref deviceReport);
        }
    }
}
