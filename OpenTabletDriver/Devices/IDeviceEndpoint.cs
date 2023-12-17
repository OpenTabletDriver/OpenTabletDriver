using System.Collections.Generic;
using JetBrains.Annotations;

namespace OpenTabletDriver.Devices
{
    /// <summary>
    /// A USB device endpoint.
    /// </summary>
    [PublicAPI]
    public interface IDeviceEndpoint
    {
        /// <summary>
        /// The device's product identifier. This is assigned by the vendor.
        /// </summary>
        int ProductID { get; }

        /// <summary>
        /// The device's vendor identifier. This is assigned by the USB-IF.
        /// </summary>
        /// <remarks>
        /// See https://www.usb.org/developers for more information.
        /// </remarks>
        int VendorID { get; }

        /// <summary>
        /// The length of the device endpoint's input reports.
        /// </summary>
        int InputReportLength { get; }

        /// <summary>
        /// The length of the device endpoint's output reports.
        /// </summary>
        int OutputReportLength { get; }

        /// <summary>
        /// The length of the device endpoint's feature reports.
        /// </summary>
        int FeatureReportLength { get; }

        /// <summary>
        /// The device's manufacturer.
        /// </summary>
        string? Manufacturer { get; }

        /// <summary>
        /// The device's product name assigned by the vendor.
        /// </summary>
        string? ProductName { get; }

        /// <summary>
        /// The device's product name assigned by the vendor.
        /// </summary>
        string? FriendlyName { get; }

        /// <summary>
        /// The device's serial number, if applicable.
        /// </summary>
        string? SerialNumber { get; }

        /// <summary>
        /// The internal system device path.
        /// This often varies by platform and host device.
        /// </summary>
        string DevicePath { get; }

        /// <summary>
        /// Whether you can open the device endpoint stream.
        /// </summary>
        /// <remarks>
        /// When false, it may indicate permission issues or that the device is exclusively opened elsewhere.
        /// </remarks>
        bool CanOpen { get; }

        /// <summary>
        /// Gets the device's attributes.
        /// </summary>
        IDictionary<string, string>? DeviceAttributes { get; }

        /// <summary>
        /// Opens the device endpoint stream for read/write.
        /// </summary>
        /// <returns>
        /// A <see cref="IDeviceEndpointStream"/> if successful, otherwise null.
        /// </returns>
        IDeviceEndpointStream? Open();

        /// <summary>
        /// Requests a USB device string embedded in the device firmware.
        /// </summary>
        /// <param name="index">
        /// The index in which to query.
        /// </param>
        /// <returns>
        /// The requested string from device firmware, or null.
        /// </returns>
        string? GetDeviceString(byte index);
    }
}
