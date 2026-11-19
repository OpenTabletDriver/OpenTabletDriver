using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenTabletDriver.Desktop.Diagnostics;
using OpenTabletDriver.Desktop.Reflection.Metadata;
using OpenTabletDriver.Desktop.RPC;
using OpenTabletDriver.Desktop.Updater;
using OpenTabletDriver.Plugin.Devices;
using OpenTabletDriver.Plugin.Logging;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Contracts
{
    /// <summary>
    /// Driver Daemon contract for interfacing with clients and configuring an underlying driver.
    /// Handles savable user-configurable settings and connects the driver to the final output handler and allows for
    /// plugins and a user-configurable processing pipeline, while allowing clients to inspect parsed reports via <see cref="DeviceReport"/>
    /// </summary>
    /// <remarks>
    public interface IDriverDaemon
    {
        /// <summary>
        /// Event for log messages that are intended to be displayed in a client log view.
        /// </summary>
        event EventHandler<LogMessage>? Message;

        /// <summary>
        /// Event for parsed but unprocessed (pre-filter) device report data.
        /// </summary>
        event EventHandler<DebugReportData>? DeviceReport;

        /// <summary>
        /// Event for tablets changing connection state.
        /// </summary>
        event EventHandler<IEnumerable<TabletReference>>? TabletsChanged;
        event EventHandler? Resynchronize;

        /// <summary>
        /// Send a message for the daemon to log.
        /// </summary>
        /// <param name="message">The <see cref="LogMessage"/> to log.</param>
        Task WriteMessage(LogMessage message);

        Task LoadPlugins();

        /// <summary>
        /// Install a plugin from a file path.
        /// </summary>
        /// <param name="filePath">The path to the plugin (.zip or .dll).</param>
        /// <returns><c>true</c> if installing the plugin succeeded.</returns>
        /// <remarks>This blindly installs plugins without verifying them for compatibility or checksumming.
        ///     If installing from an online source, prefer using <see cref="DownloadPlugin"/> instead where possible.</remarks>
        Task<bool> InstallPlugin(string filePath);

        /// <summary>
        /// Uninstall an installed plugin.
        /// </summary>
        /// <param name="pluginDirectoryName">The directory name of the local plugin.</param>
        /// <returns><c>true</c> if uninstalling the plugin succeeded.</returns>
        Task<bool> UninstallPlugin(string friendlyName);

        /// <summary>
        /// Install a plugin from associated <see cref="PluginMetadata"/>.
        /// </summary>
        /// <param name="metadata">The specifications of the plugin to download.</param>
        /// <returns><c>true</c> if downloading and installing the plugin succeeded, <c>false</c> if installing the plugin failed.</returns>
        /// <remarks><see cref="PluginMetadata.DownloadUrl"/> must be a valid URL. May throw exceptions if downloading is unsuccessful.</remarks>
        Task<bool> DownloadPlugin(PluginMetadata metadata);

        /// <summary>
        /// Gets all connected HIDs, including potentially unopenable and non-tablet devices.
        /// </summary>
        /// <returns>Connected device endpoints.</returns>
        Task<IEnumerable<SerializedDeviceEndpoint>> GetDevices();

        /// <summary>
        /// Gets all connected tablets that have been detected by the daemon via <see cref="ReinitializeTablets"/>.
        /// </summary>
        /// <returns>Connected and detected tablets.</returns>
        Task<IEnumerable<TabletReference>> GetTablets();

        /// <summary>
        /// Redetect tablets.
        /// Can be used as an escape hatch to fix unresponsive tablets, or invalid pipeline states that the daemon didn't automatically recover from.
        /// </summary>
        /// <returns>The list of detected tablets.</returns>
        /// <remarks>You almost always want to also call <see cref="SetSettings"/> with <see cref="GetSettings"/> as parameter afterwards to restore the pipeline.</returns>
        Task<IEnumerable<TabletReference>> DetectTablets();

        /// <summary>
        /// Reconfigure the driver pipeline with new settings.
        /// </summary>
        /// <param name="settings">The new settings to set.</param>
        Task SetSettings(Settings settings);

        /// <summary>
        /// Get the <see cref="Settings"/> that the driver pipeline was last configured with.
        /// </summary>
        /// <returns>The currently configured settings.</returns>
        Task<Settings> GetSettings();

        /// <summary>
        /// Reset the settings to default, discarding any earlier <see cref="SetSettings"/> invocations.
        /// </summary>
        Task ResetSettings();

        /// <summary>
        /// Get the current <see cref="AppInfo"/> instance associated with the daemon, containing paths to folders used.
        /// </summary>
        /// <returns>The currently active <see cref="AppInfo"/> instance. This is unlikely to change on runtime.</returns>
        /// <remarks>Clients relying on these should consider whether a more appropriate interface method can be used.
        /// Alternatively, if a new interface method can be added that solves the same, then a feature request could be added.
        /// It is not recommended to read OpenTabletDriver folders directly.</remarks>
        Task<AppInfo> GetApplicationInfo();

        /// <summary>
        /// Toggle emitting debug reports via <see cref="DeviceReport"/>
        /// </summary>
        /// <param name="isEnabled"><c>true</c> if subscribers should get notified of debug reports.</param>
        // TODO: consider retiring this and simply having the subscription be the request for enabling tablet debugging?
        Task SetTabletDebug(bool isEnabled);

        /// <summary>
        /// Request a USB Device String Descriptor from the specified device and index.
        /// </summary>
        /// <param name="vendorID">The USB vendor ID of the device.</param>
        /// <param name="productID">The USB product ID of the device.</param>
        /// <param name="index">The USB device string descriptor index.</param>
        /// <returns>The nullable UTF-8 formatted string associated with the USB device string descriptor index.</returns>
        /// <remarks>Will throw exceptions if the device is invalid, or if the request could not be initialized correctly.</remarks>
        // TODO: Add ValueTask<Dictionary<int, string?>> RequestAllDeviceStrings(int vendorID, int productID); ?
        Task<string> RequestDeviceString(int vendorID, int productID, int index);

        /// <summary>
        /// Send a HIDIOCSFEATURE / SetFeature report to a USB device.
        /// </summary>
        /// <param name="vendorID">The USB vendor ID of the device.</param>
        /// <param name="productID">The USB product ID of the device.</param>
        /// <param name="strInterface">The interface matched by <c>USB_INTERFACE_NUMBER</c> of the diagnostics file.</param>
        /// <param name="initData">The byte string to send.</param>
        /// <remarks>Will throw exception if the device is invalid, or if sending the init data failed.</remarks>
        Task SendFeatureInit(int vendorID, int productID, string strInterface, byte[] initData);

        /// <summary>
        /// Send a raw report to a USB device.
        /// </summary>
        /// <param name="vendorID">The USB vendor ID of the device.</param>
        /// <param name="productID">The USB product ID of the device.</param>
        /// <param name="strInterface">The interface matched by <c>USB_INTERFACE_NUMBER</c> of the diagnostics file.</param>
        /// <param name="initData">The byte string to send.</param>
        /// <remarks>Will throw exception if the device is invalid, or if sending the init data failed.</remarks>
        Task SendOutputInit(int vendorID, int productID, string strInterface, byte[] initData);

        /// <summary>
        /// Get the current daemon log.
        /// </summary>
        /// <returns>The entire current daemon log.</returns>
        Task<IEnumerable<LogMessage>> GetCurrentLog();

        /// <summary>
        /// Get diagnostics information associated with the driver.
        /// </summary>
        /// <returns>Details about the drivers view of the system as well as all logs.</returns>
        Task<DiagnosticInfo> GetDiagnosticInfo();

        /// <summary>
        /// Check whether updates are available.
        /// </summary>
        /// <returns>Information associated with the update, if there are any updates.</returns>
        /// <remarks>This is likely to change when/if auto-updater is implemented. Avoid using this directly.</remarks>
        Task<SerializedUpdateInfo?> CheckForUpdates();

        /// <summary>
        /// Installs the latest update available.
        /// </summary>
        Task InstallUpdate();

        Task ForceResynchronize();
    }
}
