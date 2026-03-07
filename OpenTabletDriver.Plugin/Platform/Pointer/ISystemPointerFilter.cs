using System.Collections.Generic;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Plugin.Platform.Pointer
{
    public interface ISystemPointerFilter
    {
        bool Enabled { get; }

        void ConnectionStatusChanged(string deviceName, List<DeviceIdentifier> identifiers, bool connected);
    }
}
