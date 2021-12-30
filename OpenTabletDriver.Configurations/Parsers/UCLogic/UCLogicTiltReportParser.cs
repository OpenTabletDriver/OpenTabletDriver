using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.UCLogic
{
    public class UCLogicTiltReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] data)
        {
            if (data[1].IsBitSet(6))
                return new UCLogicAuxReport(data);
            else
                return new TiltTabletReport(data);
        }
    }
}
