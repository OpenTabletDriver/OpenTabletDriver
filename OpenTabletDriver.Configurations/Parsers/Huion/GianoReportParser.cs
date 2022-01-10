using OpenTabletDriver.Configurations.Parsers.UCLogic;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Huion
{
    public class GianoReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] data)
        {
            if (data[1].IsBitSet(5) && data[1].IsBitSet(6))
                return new UCLogicAuxReport(data);
            else
                return new GianoReport(data);
        }
    }
}
