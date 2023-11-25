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
        event EventHandler<InputDevice> InputDeviceAdded;
        event EventHandler<InputDevice> InputDeviceRemoved;
        ImmutableArray<InputDevice> InputDevices { get; }
        IReportParser<IDeviceReport> GetReportParser(DeviceIdentifier identifier);
        void ScanDevices();
    }
}
