using System;
using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Touch;

namespace OpenTabletDriver.Configurations.Parsers.Wacom
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public class Wacom64bAuxReportParser : IReportParser
    {
        private TouchPoint?[]? _prevTouches;

        public bool Parse(scoped ReadOnlySpan<byte> raw, scoped ref DeviceReport deviceReport) =>
            WacomTouchReport.Parse(raw, ref _prevTouches);
    }
}
