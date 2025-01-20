using OpenTabletDriver.Configurations.Parsers.UCLogic;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Huion
{
    public class HuionAuxReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] data)
        {
            return data[1] switch
            {
                0xE0 => new UCLogicAuxReport(data),
                0xF0 => new HuionWheelReport(data, ref _prevWheelPosition),
                _ => new TiltTabletReport(data)
            };
        }

        private uint? _prevWheelPosition = null;
    }
}
