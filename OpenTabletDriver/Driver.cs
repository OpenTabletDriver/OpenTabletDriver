#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using OpenTabletDriver.Attributes;
using OpenTabletDriver.Components;
using OpenTabletDriver.Devices;
using OpenTabletDriver.Interop;
using OpenTabletDriver.Tablet;
namespace OpenTabletDriver
{
    /// <summary>
    /// A base implementation of <see cref="IDriver"/>.
    /// </summary>
    [PublicAPI]
    public class Driver : IDriver, IDisposable
    {
        private readonly ICompositeDeviceHub _compositeDeviceHub;
        private readonly IReportParserProvider _reportParserProvider;
        private readonly IDeviceConfigurationProvider _deviceConfigurationProvider;

        public Driver(
            ICompositeDeviceHub deviceHub,
            IReportParserProvider reportParserProvider,
            IDeviceConfigurationProvider configurationProvider
        )
        {
            _compositeDeviceHub = deviceHub;
            _reportParserProvider = reportParserProvider;
            _deviceConfigurationProvider = configurationProvider;
        }

        public event EventHandler<IEnumerable<InputDevice>>? InputDevicesChanged;

        public InputDeviceCollection InputDevices { get; } = new InputDeviceCollection();

        public IEnumerable<TabletConfiguration> TabletConfigurations =>
            _deviceConfigurationProvider.TabletConfigurations;

        public IReportParser<IDeviceReport> GetReportParser(DeviceIdentifier identifier)
        {
            return _reportParserProvider.GetReportParser(identifier.ReportParser);
        }

        public virtual void Detect()
        {
            bool success = false;

            Log.Write("Detect", "Searching for tablets...");

            InputDevices.Clear();
            foreach (var config in _deviceConfigurationProvider.TabletConfigurations)
            {
                if (Match(config) is InputDevice tree)
                {
                    success = true;
                    InputDevices.Add(tree);

                    tree.Disconnected += (sender, e) =>
                    {
                        InputDevices.Remove(tree);
                        InputDevicesChanged?.Invoke(this, InputDevices);
                    };
                }
            }

            InputDevicesChanged?.Invoke(this, InputDevices);

            if (!success)
            {
                Log.Write("Detect", "No tablets were detected.");
            }
        }

        protected virtual InputDevice? Match(TabletConfiguration config)
        {
            Log.Debug("Detect", $"Searching for tablet '{config.Name}'");
            try
            {
                var devices = new List<InputDeviceEndpoint>();
                if (MatchDevice(config, config.DigitizerIdentifiers) is InputDeviceEndpoint digitizer)
                {
                    Log.Write("Detect", $"Found tablet '{config.Name}'");
                    devices.Add(digitizer);

                    if (config.AuxiliaryDeviceIdentifiers.Any())
                    {
                        if (MatchDevice(config, config.AuxiliaryDeviceIdentifiers) is InputDeviceEndpoint aux)
                            devices.Add(aux);
                        else
                            Log.Write("Detect", "Failed to find auxiliary device, express keys may be unavailable.", LogLevel.Warning);
                    }

                    return new InputDevice(config, devices);
                }
            }
            catch (IOException iex) when (iex.Message.Contains("Unable to open HID class device")
                && SystemInterop.CurrentPlatform == SystemPlatform.Linux)
            {
                Log.Write(
                    "Driver",
                    "The current user does not have the permissions to open the device stream. " +
                    "Follow the instructions from https://opentabletdriver.net/Wiki/FAQ/Linux#fail-device-streams to resolve this issue.",
                    LogLevel.Error
                );
            }
            catch (ArgumentOutOfRangeException aex) when (aex.Message.Contains("Value range is [0, 15]")
                && SystemInterop.CurrentPlatform == SystemPlatform.Linux)
            {
                Log.Write(
                    "Driver",
                    "Device is currently in use by another kernel module. " +
                    "Follow the instructions from https://opentabletdriver.net/Wiki/FAQ/Linux#argumentoutofrangeexception to resolve this issue.",
                    LogLevel.Error
                );
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
            return null;
        }

        private InputDeviceEndpoint? MatchDevice(TabletConfiguration config, IList<DeviceIdentifier> identifiers)
        {
            foreach (var identifier in identifiers)
            {
                var matches = GetMatchingDevices(config, identifier);

                if (matches.Count() > 1)
                    Log.Write("Detect", "More than 1 matching device has been found.", LogLevel.Warning);

                foreach (IDeviceEndpoint dev in matches)
                {
                    try
                    {
                        return new InputDeviceEndpoint(this, dev, config, identifier);
                    }
                    catch (Exception ex)
                    {
                        Log.Exception(ex);
                        continue;
                    }
                }
            }
            return null;
        }

        private IEnumerable<IDeviceEndpoint> GetMatchingDevices(TabletConfiguration configuration, DeviceIdentifier identifier)
        {
            return from device in _compositeDeviceHub.GetDevices()
                   where identifier.VendorID == device.VendorID
                   where identifier.ProductID == device.ProductID
                   where device.CanOpen
                   where identifier.InputReportLength == null || identifier.InputReportLength == device.InputReportLength
                   where identifier.OutputReportLength == null || identifier.OutputReportLength == device.OutputReportLength
                   where DeviceMatchesStrings(device, identifier.DeviceStrings)
                   where DeviceMatchesAttribute(device, configuration.Attributes)
                   select device;
        }

        private bool DeviceMatchesStrings(IDeviceEndpoint device, IDictionary<byte, string>? deviceStrings)
        {
            if (deviceStrings == null || deviceStrings.Count == 0)
                return true;

            foreach (var matchQuery in deviceStrings)
            {
                try
                {
                    // Iterate through each device string, if one doesn't match then its the wrong configuration.
                    var input = device.GetDeviceString(matchQuery.Key) ?? string.Empty;
                    var pattern = matchQuery.Value;
                    if (!Regex.IsMatch(input, pattern))
                        return false;
                }
                catch (Exception ex)
                {
                    Log.Exception(ex);
                    return false;
                }
            }
            return true;
        }

        private bool DeviceMatchesAttribute(IDeviceEndpoint device, Dictionary<string, string> attributes)
        {
            switch (SystemInterop.CurrentPlatform)
            {
                case SystemPlatform.Windows:
                {
                    var devName = device.DevicePath;

                    var interfaceMatches = !attributes.ContainsKey("WinInterface") || Regex.IsMatch(devName, $"&mi_{attributes["WinInterface"]}");
                    var keyMatches = !attributes.ContainsKey("WinUsage") || Regex.IsMatch(devName, $"&col{attributes["WinUsage"]}");

                    return interfaceMatches && keyMatches;
                }
                case SystemPlatform.MacOS:
                {
                    var devName = device.DevicePath;
                    bool interfaceMatches = !attributes.ContainsKey("MacInterface") || Regex.IsMatch(devName, $"IOUSBHostInterface@{attributes["MacInterface"]}");
                    return interfaceMatches;
                }
                default:
                {
                    return true;
                }
            }
        }

        public void ConnectLegacyDevice(Uri path, TabletConfiguration config)
        {
            ILegacyDeviceHub? selectedHub = null;
            foreach (ILegacyDeviceHub hub in _compositeDeviceHub.LegacyDeviceHubs)
            {
                string? protocol = hub.GetType().GetCustomAttribute<LegacyDeviceHubAttribute>()?.Protocol;
                if (protocol == null)
                    continue;

                if (path.Scheme == protocol)
                {
                    selectedHub = hub;
                    break;
                }
            }
            
            string fullPath = path.ToString();
            
            int pathStart = path.Scheme.Length + 2;
            int pathEnd = fullPath[fullPath.Length-1] == '/' ? 0 : 1;
            string actualPath = fullPath.Substring(pathStart, fullPath.Length - (pathStart + pathEnd));

            if (selectedHub == null || !selectedHub.TryGetDevice(actualPath, out IDeviceEndpoint endpoint))
            {
                throw new ArgumentException();
            }

            List<InputDeviceEndpoint> endpoints = new List<InputDeviceEndpoint>();

            foreach (var id in config.DigitizerIdentifiers)
            {
                endpoints.Add(new InputDeviceEndpoint(this, endpoint, config, id));
            }


            InputDevice dev = new InputDevice(config, endpoints);

            InputDevices.Add(dev);

            dev.Disconnected += (sender, e) =>
            {
                InputDevices.Remove(dev);
                InputDevicesChanged?.Invoke(this, InputDevices);
            };
        }

        public void Dispose()
        {
            foreach (InputDevice tree in InputDevices.ToList())
                foreach (InputDeviceEndpoint dev in tree.Endpoints.ToList())
                    dev.Dispose();
        }
    }
}
