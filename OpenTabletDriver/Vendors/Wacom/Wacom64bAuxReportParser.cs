using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Vendors.Wacom
{
    public class Wacom64bAuxReportParser : IReportParser<IDeviceReport>
    {
        public virtual IDeviceReport Parse(byte[] data)
        {
            switch (data[2])
            {
                case 0x80:
                    return new AuxReport
                    {
                        Raw = data,
                        AuxButtons = new bool[]
                        {
                            (data[3] & (1 << 0)) != 0,
                            (data[3] & (1 << 1)) != 0,
                            (data[3] & (1 << 2)) != 0,
                            (data[3] & (1 << 3)) != 0
                        }
                    };
                default:
                    return new WacomTouchReport(data);
            }
        }
    }
}
