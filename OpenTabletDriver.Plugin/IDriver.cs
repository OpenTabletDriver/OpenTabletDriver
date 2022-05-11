using System;
using System.Collections.Generic;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Plugin
{
    public interface IDriver
    {
        /// <summary>
        /// Invoked whenever a tablet is either detected or is disconnected.
        /// </summary>
        event EventHandler<IEnumerable<TabletReference>> TabletsChanged;

        /// <summary>
        /// The currently active and detected tablets.
        /// </summary>
        IEnumerable<TabletReference> Tablets { get; }

        /// <summary>
        /// Attempts to detect a tablet.
        /// </summary>
        /// <returns>True if any configuration successfully matched.</returns>
        bool Detect();

        /// <summary>
        /// Retrieve and construct the the report parser for an identifier.
        /// </summary>
        /// <param name="identifier">The identifier to retrieve the report parser path from.</param>
        IReportParser<IDeviceReport> GetReportParser(DeviceIdentifier identifier);

        void ConnectLegacyDevice(Uri uri, TabletConfiguration config);
    }
}
