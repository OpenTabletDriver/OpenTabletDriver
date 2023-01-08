using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using OpenTabletDriver.Devices;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver
{
    /// <summary>
    /// An input device endpoint.
    /// </summary>
    [PublicAPI]
    public class InputDeviceEndpoint : DeviceReader<IDeviceReport>
    {
        public InputDeviceEndpoint(IDriver driver, IDeviceEndpoint device, TabletConfiguration configuration, DeviceIdentifier identifier)
            : base(device, driver.GetReportParser(identifier))
        {
            if (driver == null || device == null || configuration == null || identifier == null)
            {
                string argumentName = driver == null ? nameof(driver) :
                    device == null ? nameof(device) :
                    configuration == null ? nameof(configuration) :
                    nameof(identifier);
                throw new ArgumentNullException(argumentName);
            }

            Endpoint = device;
            Configuration = configuration;
            Identifier = identifier;
        }

        /// <summary>
        /// The tablet configuration referring to this device.
        /// </summary>
        public TabletConfiguration Configuration { get; }

        /// <summary>
        /// The device identifier used to detect this device.
        /// </summary>
        /// <value></value>
        public DeviceIdentifier Identifier { get; }

        protected override bool Initialize()
        {
            if (!base.Initialize())
                return false;

            Log.Debug("Device", $"Initializing device '{Endpoint.FriendlyName}' {Endpoint.DevicePath}");
            Log.Debug("Device", $"Using report parser type '{Identifier.ReportParser}'");

            foreach (byte index in Identifier.InitializationStrings ?? new List<byte>())
            {
                Endpoint.GetDeviceString(index);
                Log.Debug("Device", $"Initialized string index {index}");
            }

            foreach (var report in Identifier.FeatureInitReport ?? new List<byte[]>())
            {
                if (report.Length == 0)
                    continue;

                try
                {
                    ReportStream!.SetFeature(report);
                    Log.Debug("Device", "Set device feature: " + BitConverter.ToString(report));
                }
                catch
                {
                    Log.Write("Device", "Failed to set device feature: " + BitConverter.ToString(report), LogLevel.Warning);
                }
            }

            foreach (var report in Identifier.OutputInitReport ?? new List<byte[]>())
            {
                if (report.Length == 0)
                    continue;

                try
                {
                    ReportStream!.Write(report);
                    Log.Debug("Device", "Set device output: " + BitConverter.ToString(report));
                }
                catch
                {
                    Log.Write("Device", "Failed to set device output: " + BitConverter.ToString(report), LogLevel.Warning);
                }
            }

            return true;
        }
    }
}
