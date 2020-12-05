using System;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Platform.Display;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Plugin
{
    public interface IDriver
    {
        event EventHandler<bool> Reading;
        event EventHandler<IDeviceReport> ReportRecieved;
        event EventHandler<TabletState> TabletChanged;

        bool EnableInput { set; get; }
        TabletState Tablet { get; }
        IOutputMode OutputMode { set; get; }

        void HandleReport(IDeviceReport report);
        bool TryMatch(TabletConfiguration tablet);
    }
}
