using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Configurations.Parsers.UCLogic;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Huion
{
    /// <summary>
    /// <see cref="UCLogicTiltReportParser"/> for Huion tablets with an absolute touch strip,
    /// which reports its position in a separate report identified by <c>0xF0</c> in byte 1.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public class HuionTouchStripReportParser : IReportParser<IDeviceReport>
    {
        private const byte TOUCH_STRIP = 0xF0;

        private readonly UCLogicTiltReportParser _tiltReportParser = new();

        public IDeviceReport Parse(byte[] data)
        {
            if (data[1] == TOUCH_STRIP)
                return new HuionWheelReport(data);

            return _tiltReportParser.Parse(data);
        }
    }
}
