using System.Diagnostics.CodeAnalysis;

namespace OpenTabletDriver.Plugin.Tablet
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public class TabletReportParser : IReportParser<ITabletReport>
    {
        public virtual ITabletReport Parse(byte[] data)
        {
            return new TabletReport(data);
        }
    }
}
