using System;
using System.Diagnostics.CodeAnalysis;

namespace OpenTabletDriver.Plugin.Tablet
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public class AuxReportParser : IReportParser
    {
        public bool Parse(scoped in ReadOnlySpan<byte> raw, scoped ref DeviceReport deviceReport)
        {
            return AuxReport.Parse(raw, ref deviceReport);
        }
    }
}
