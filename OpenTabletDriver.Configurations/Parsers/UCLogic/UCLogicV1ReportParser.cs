using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.UCLogic
{
    public class UCLogicV1ReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] data)
        {
            if (data[1] == 0xE0)
                return new UCLogicAuxReport(data);
            else if (data[1].IsBitSet(6))
                return new TabletReport(data);
            else
                return new OutOfRangeReport(data);
        }
    }
}
