using System;
using JetBrains.Annotations;

namespace OpenTabletDriver.Devices
{
    /// <summary>
    /// A USB device endpoint stream.
    /// </summary>
    [PublicAPI]
    public interface IDeviceEndpointStream : IDisposable
    {
        /// <summary>
        /// Reads the next report from the device endpoint.
        /// </summary>
        /// <remarks>
        /// This method blocks until a full report is received.
        /// </remarks>
        /// <returns>
        /// A device report.
        /// </returns>
        byte[] Read();

        /// <summary>
        /// Writes to the device endpoint stream.
        /// </summary>
        /// <remarks>
        /// This method blocks until the write is complete.
        /// </remarks>
        /// <param name="buffer">
        /// The data in which to write to the device endpoint stream.
        /// </param>
        void Write(byte[] buffer);

        /// <summary>
        /// Invokes GetFeature on the device endpoint.
        /// </summary>
        /// <param name="buffer">
        /// The data in which to pass.
        /// </param>
        void GetFeature(byte[] buffer);

        /// <summary>
        /// Invokes SetFeature on the device endpoint.
        /// </summary>
        /// <param name="buffer">
        /// The data in which to pass.
        /// </param>
        void SetFeature(byte[] buffer);
    }
}
