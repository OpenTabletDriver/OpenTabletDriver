using OpenTabletDriver.Configurations.Parsers.UCLogic;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Huion
{
    public class InspiroyReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] data)
        {
            switch (data[1])
            {
                case 0xe0:
                    return new UCLogicAuxReport(data);
                case 0xe3:
                    // Group buttons, no way to use them properly for now
                    return new UCLogicAuxReport(data);
                case 0xf1:
                    // Wheel data, reported in data[5], ignoring
                    return new DeviceReport(data);
                case 0x00:
                    return new OutOfRangeReport(data);
            };

            if (data[1].IsBitSet(7))
            {
                return new TiltTabletReport(data);
            }

            return new DeviceReport(data);
        }
    }
}
