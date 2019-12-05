using System.Linq;
using TabletDriverLib.Tablet;

namespace TabletDriverLib.Compatibility
{
    public class WacomCompatibilityLayer : ICompatibilityLayer<ITabletReport>
    {
        public ITabletReport Fix(byte[] report)
        {
            return new TabletReport(report.Skip(1) as byte[]);
        }
    }
}