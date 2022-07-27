using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver
{
    /// <summary>
    /// A base driver interface.
    /// </summary>
    [PublicAPI]
    public interface IDriver
    {
        event EventHandler<IEnumerable<InputDevice>>? InputDevicesChanged;
        InputDeviceCollection InputDevices { get; }
        IReportParser<IDeviceReport> GetReportParser(DeviceIdentifier identifier);
        void Detect();
    }
}
