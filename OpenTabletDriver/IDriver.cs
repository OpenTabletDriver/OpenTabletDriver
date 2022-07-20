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
        /// <summary>
        /// Invoked whenever a tablet is either detected or is disconnected.
        /// </summary>
        event EventHandler<IEnumerable<InputDevice>> InputDevicesChanged;

        /// <summary>
        /// The currently active and detected tablets.
        /// </summary>
        InputDeviceCollection InputDevices { get; }

        /// <summary>
        /// Attempts to detect a tablet.
        /// </summary>
        /// <returns>True if any configuration successfully matched.</returns>
        void Detect();

        /// <summary>
        /// Retrieve and construct the the report parser for an identifier.
        /// </summary>
        /// <param name="identifier">The identifier to retrieve the report parser path from.</param>
        IReportParser<IDeviceReport> GetReportParser(DeviceIdentifier identifier);
    }
}
