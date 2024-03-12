using System;

namespace OpenTabletDriver.SystemDrivers
{
    [Flags]
    public enum DriverStatus
    {
        /// <summary>
        /// The driver is actively accessing the device and sends input to OS.
        /// </summary>
        Active = 1,

        /// <summary>
        /// The driver is potentially blocking OpenTabletDriver from accessing the device.
        /// </summary>
        Blocking = 1 << 1,

        /// <summary>
        /// The existence of this driver causes OpenTabletDriver to have flaky support to the device.
        /// </summary>
        Flaky = 1 << 2,
    }
}
