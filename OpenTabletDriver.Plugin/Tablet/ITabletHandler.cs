using System;
using OpenTabletDriver.Plugin.Output;

namespace OpenTabletDriver.Plugin.Tablet
{
    public interface ITabletHandler : IDisposable
    {
        TabletState TabletState { get; init; }
        TabletHandlerID InstanceID { get; init; }
        IOutputMode OutputMode { get; set; }
        bool EnableInput { get; set; }
        Action<IOutputMode, IDeviceReport> HandleReport { get; set; }

        event EventHandler<TabletHandlerID> Disconnected;

        bool Initialize();
    }
}