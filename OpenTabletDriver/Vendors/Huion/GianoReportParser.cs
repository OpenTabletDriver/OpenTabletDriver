using OpenTabletDriver.Tablet;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Vendors.UCLogic;

namespace OpenTabletDriver.Vendors.Huion
{
    public class GianoReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] data)
        {
            var isAuxReport = ((data[1] & (1 << 5)) != 0) & ((data[1] & (1 << 6)) != 0);
            if (isAuxReport)
                return new UCLogicAuxReport(data);
            else
                return new GianoReport(data);
        }
    }
}
