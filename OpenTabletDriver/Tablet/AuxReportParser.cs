using JetBrains.Annotations;

namespace OpenTabletDriver.Tablet
{
    [PublicAPI]
    public class AuxReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] data)
        {
            return new AuxReport(data);
        }
    }
}
