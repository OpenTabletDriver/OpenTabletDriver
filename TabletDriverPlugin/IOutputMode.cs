using System.Collections.Generic;
using TabletDriverPlugin.Tablet;

namespace TabletDriverPlugin
{
    public interface IOutputMode
    {
        void Read(IDeviceReport report);
        IEnumerable<IFilter> Filters { set; get; }
        TabletProperties TabletProperties { set; get; }
    }
}