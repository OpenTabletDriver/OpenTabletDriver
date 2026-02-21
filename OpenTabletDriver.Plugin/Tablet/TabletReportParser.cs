using System.Diagnostics.CodeAnalysis;

namespace OpenTabletDriver.Plugin.Tablet
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public class TabletReportParser : IReportParser<IDeviceReport>
    {
        public virtual IDeviceReport Parse(byte[] data)
        {
            return new TabletReport(data);
        }
    }
}
