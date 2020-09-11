using System.Collections.Generic;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Plugin.Output
{
    public interface IOutputMode
    {
        void Read(IDeviceReport report);
        IEnumerable<IFilter> Filters { set; get; }
        TabletProperties TabletProperties { set; get; }
        IVirtualPointer Pointer { get; }
    }
}