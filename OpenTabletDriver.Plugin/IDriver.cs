using System;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Plugin
{
    public interface IDriver
    {
        event EventHandler<bool> Reading;
        event EventHandler<IDeviceReport> ReportRecieved;

        bool EnableInput { set; get; }
        TabletProperties TabletProperties { get; }
        IOutputMode OutputMode { set; get; }

        bool TryMatch(TabletProperties tablet);
    }
}