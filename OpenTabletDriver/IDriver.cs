using System;
using System.Collections.Immutable;
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
        event EventHandler<ImmutableArray<InputDevice>>? InputDevicesChanged;
        ImmutableArray<InputDevice> InputDevices { get; }
        IReportParser<IDeviceReport> GetReportParser(DeviceIdentifier identifier);
        void Detect();
    }
}
