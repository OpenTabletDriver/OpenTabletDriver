using System;
using System.Collections.Generic;
using OpenTabletDriver.Devices;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Devices;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver
{
    public class InputDevice : DeviceReader<IDeviceReport>
    {
        public InputDevice(IDriver driver, IDeviceEndpoint device, TabletConfiguration configuration, DeviceIdentifier identifier)
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

            Start();
        }

        /// <summary>
        /// The driver in which this <see cref="InputDevice"/> was sourced from.
        /// </summary>
        public IDriver Driver { private set; get; }

        /// <summary>
        /// The tablet configuration referring to this device.
        /// </summary>
        public TabletConfiguration Configuration { private set; get; }

        /// <summary>
        /// The device identifier used to detect this device.
        /// </summary>
        /// <value></value>
        public DeviceIdentifier Identifier { private set; get; }

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
                if (report == null || report.Length == 0)
                    continue;

                try
                {
                    ReportStream.SetFeature(report);
                    Log.Debug("Device", "Set device feature: " + BitConverter.ToString(report));
                }
                catch
                {
                    Log.Write("Device", "Failed to set device feature: " + BitConverter.ToString(report), LogLevel.Warning);
                }
            }

            foreach (var report in Identifier.OutputInitReport ?? new List<byte[]>())
            {
                if (report == null || report.Length == 0)
                    continue;

                try
                {
                    ReportStream.Write(report);
                    Log.Debug("Device", "Set device output: " + BitConverter.ToString(report));
                }
                catch
                {
                    Log.Write("Device", "Failed to set device output: " + BitConverter.ToString(report), LogLevel.Warning);
                }
            }

            return true;
        }

        private bool TryGetDeviceProperty<T>(Func<IDeviceEndpoint, T> predicate, out T value)
        {
            try
            {
                value = predicate(Endpoint);
                return true;
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
            value = default(T);
            return false;
        }

        private T SafeGetDeviceProperty<T>(Func<IDeviceEndpoint, T> predicate, T fallback) => TryGetDeviceProperty(predicate, out var val) ? val : fallback;
    }
}
