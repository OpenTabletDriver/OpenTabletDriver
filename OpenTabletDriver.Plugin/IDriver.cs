using System;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Platform.Display;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Plugin
{
    public interface IDriver
    {
        event Action<bool> Reading;
        event Action<IDeviceReport> ReportRecieved;

        bool EnableInput { set; get; }
        TabletConfiguration Tablet { get; }
        DigitizerIdentifier TabletIdentifier { get; }
        DeviceIdentifier AuxiliaryIdentifier { get; }
        IVirtualScreen VirtualScreen { get; }
        IOutputMode OutputMode { set; get; }

        void InjectReport(IDeviceReport report);
        bool TryMatch(TabletConfiguration tablet);
    }
}