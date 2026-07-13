using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Bosto
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public class Bosto13HDReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] data)
        {
            var reportType = data[0];
            
            if (reportType == 0x01)
            {
                var penReport = data[1];

                if (penReport.IsBitSet(4))
                {
                    return new Bosto13HDTabletReport(data);
                }
                else
                {
                    return new OutOfRangeReport(data);
                }
            }
            else if (reportType == 0x04)
            {
                return new BostoAuxReport(data);
            }
            else
            {
                return new DeviceReport(data);
            }
        }
    }
}
