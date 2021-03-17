using System;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Plugin
{
    public interface IDriver
    {
        /// <summary>
        /// Invoked when a device endpoint begins reading or stops being read.
        /// </summary>
        event EventHandler<bool> Reading;

        /// <summary>
        /// Invoked whenever an <see cref="IDeviceReport"/> is recieved from a device endpoint.
        /// </summary>
        event EventHandler<IDeviceReport> ReportReceived;

        /// <summary>
        /// Invoked whenever a tablet is either detected or is disconnected.
        /// </summary>
        event EventHandler<TabletState> TabletChanged;

        /// <summary>
        /// Whether to allow input to be pushed to the active output mode.
        /// </summary>
        bool EnableInput { set; get; }

        /// <summary>
        /// Whether an interpolator filter is currently active.
        /// </summary>
        /// <value></value>
        bool InterpolatorActive { get; }

        /// <summary>
        /// The currently active and detected tablet.
        /// </summary>
        TabletState Tablet { get; }

        /// <summary>
        /// The active output mode at the end of the data pipeline for all data to be processed.
        /// </summary>
        IOutputMode OutputMode { set; get; }

        /// <summary>
        /// Pushes an <see cref="IDeviceReport"/> to the active output mode.
        /// </summary>
        /// <param name="report">The <see cref="IDeviceReport"/> to push to the output mode.</param>
        void HandleReport(IDeviceReport report);

        /// <summary>
        /// Attempts to detect a tablet via the parameters set in the <see cref="TabletConfiguration"/>.
        /// </summary>
        /// <param name="tablet">The tablet configuration to match.</param>
        /// <returns>True if the tablet configuration successfully matched and a tablet was detected.</returns>
        bool TryMatch(TabletConfiguration tablet);
    }
}
