using System.Collections.Generic;
using TabletDriverPlugin.Platform.Pointer;
using TabletDriverPlugin.Tablet;

namespace TabletDriverPlugin.Output
{
    public interface IOutputMode
    {
        void Read(IDeviceReport report);
        IEnumerable<IFilter> Filters { set; get; }
        TabletProperties TabletProperties { set; get; }
        IPointerHandler PointerHandler { get; }
    }
}