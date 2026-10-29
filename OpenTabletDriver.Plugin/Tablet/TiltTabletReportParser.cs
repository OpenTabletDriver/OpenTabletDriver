using System;
using System.Diagnostics.CodeAnalysis;

namespace OpenTabletDriver.Plugin.Tablet
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public class TiltTabletReportParser : IReportParser
    {
        public bool Parse(scoped in ReadOnlySpan<byte> raw, scoped ref DeviceReport deviceReport)
        {
            return deviceReport.Continue = TiltTabletReport.Parse(raw, ref deviceReport);
        }
    }
}
