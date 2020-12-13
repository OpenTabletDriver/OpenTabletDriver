using System.Collections.Generic;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Plugin.Output
{
    public interface IOutputMode
    {
        void Read(IDeviceReport report);
        IList<IFilter> Filters { set; get; }
        TabletState Tablet { set; get; }
    }
}
