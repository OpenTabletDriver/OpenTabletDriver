using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
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
        private readonly object _detectLock = new();
        private ImmutableArray<InputDevice> _inputDevices = ImmutableArray<InputDevice>.Empty;
        private Dictionary<(int, int), List<TabletConfiguration>> _configHashMap;

        public Driver(
            ICompositeDeviceHub deviceHub,
            IReportParserProvider reportParserProvider,
            IDeviceConfigurationProvider configurationProvider
        )
        {
            _compositeDeviceHub = deviceHub;
            _reportParserProvider = reportParserProvider;
            _deviceConfigurationProvider = configurationProvider;

            if (_deviceConfigurationProvider.RaisesTabletConfigurationsChanged)
            {
                _deviceConfigurationProvider.TabletConfigurationsChanged += configs =>
                {
                    _configHashMap = ConstructConfigHashMap(configs);
                    Detect();
                };
            }

            _configHashMap = ConstructConfigHashMap(_deviceConfigurationProvider.TabletConfigurations);
        }

        public event EventHandler<ImmutableArray<InputDevice>>? InputDevicesChanged;

        public ImmutableArray<InputDevice> InputDevices => _inputDevices;

        public IReportParser<IDeviceReport> GetReportParser(DeviceIdentifier identifier)
        {
            return _reportParserProvider.GetReportParser(identifier.ReportParser);
        }

        public virtual void Detect()
        {
            lock (_detectLock)
            {
                Log.Write("Detect", "Searching for tablets...");

                // save a reference to avoid potential _configHashMap changes during detection
                var tabletConfigurations = _configHashMap;

                // the following detection algorithm exploits the fact that OTD only ever uses one digitizer and one
                // auxiliary per tablet device. multiple devices of the exact same model will be detected properly
                // however current OTD API design inhibits the ability to distinguish between them.

                // create a hash map of configurations to their input device endpoint pairs
                var inputDeviceEndpoints = new Dictionary<TabletConfiguration, List<InputDeviceEndpointPair>>();

                // loop over all devices
                foreach (var device in _compositeDeviceHub.GetDevices())
                {
                    try
                    {
                        var deviceHash = (device.VendorID, device.ProductID);
                        var deviceName = device.FriendlyName == null
                            ? device.DevicePath
                            : $"{device.FriendlyName} ({device.DevicePath})";

                        // loop over configurations that has an identifier that matches the device's VID/PID
                        if (!tabletConfigurations.TryGetValue(deviceHash, out var candidateConfigs))
                            continue;

                        foreach (var candidateConfig in candidateConfigs)
                        {
                            ref var pairList = ref CollectionsMarshal.GetValueRefOrAddDefault(inputDeviceEndpoints, candidateConfig, out _);
                            Log.Debug("Detect", $"Attempting to match config '{candidateConfig.Name}' to device '{deviceName}'");

                            // check if the device matches a digitizer identifier
                            if (TryMatch(device, candidateConfig, candidateConfig.DigitizerIdentifiers, out var digitizerEndpoint))
                            {
                                // find the first pair that has no digitizer endpoint
                                // if none is found, create a new pair
                                pairList ??= new List<InputDeviceEndpointPair>();
                                var pairIndex = pairList.FindIndex(p => p.Digitizer is null);

                                if (pairIndex == -1)
                                {
                                    pairList.Add(new InputDeviceEndpointPair());
                                    pairIndex = pairList.Count - 1;
                                }

                                var pair = pairList[pairIndex];
                                pair.Digitizer = digitizerEndpoint;
                                Log.Debug("Detect", $"Found '{candidateConfig.Name}' digitizer: '{deviceName}'");
                                break;
                            }

                            // check if the device matches an auxiliary identifier
                            if (TryMatch(device, candidateConfig, candidateConfig.AuxiliaryDeviceIdentifiers, out var auxEndpoint))
                            {
                                // find the first pair that has no auxiliary endpoint
                                // if none is found, create a new pair
                                pairList ??= new List<InputDeviceEndpointPair>();
                                var pairIndex = pairList.FindIndex(p => p.Auxiliary is null);

                                if (pairIndex == -1)
                                {
                                    pairList.Add(new InputDeviceEndpointPair());
                                    pairIndex = pairList.Count - 1;
                                }

                                var pair = pairList[pairIndex];
                                pair.Auxiliary = auxEndpoint;
                                Log.Debug("Detect", $"Found '{candidateConfig.Name}' auxiliary: '{deviceName}'");
                                break;
                            }
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
                        Log.Exception(ex, LogLevel.Warning);
                    }
                }

                var deviceBuilder = ImmutableArray.CreateBuilder<InputDevice>(inputDeviceEndpoints.Count);
                foreach (var (config, pairList) in inputDeviceEndpoints.Where(p => p.Value is not null))
                {
                    foreach (var pair in pairList)
                    {
                        if (pair.Digitizer is null)
                        {
                            Log.Write("Detect", $"Digitizer device not found for tablet '{config.Name}', skipping...", LogLevel.Warning);
                            continue;
                        }

                        Log.Write("Detect", $"Found tablet '{config.Name}'");
                        var device = new InputDevice(config, pair.Digitizer, pair.Auxiliary);

                        if (config.AuxiliaryDeviceIdentifiers.Any() && pair.Auxiliary is null)
                        {
                            Log.Write("Detect", $"Auxiliary device not found for tablet '{config.Name}', express keys may not function properly", LogLevel.Warning);
                        }

                        device.StateChanged += (sender, e) =>
                        {
                            if (e < InputDeviceState.Disconnected)
                                return;

                            // save the resulting immutable array for later use.
                            ImmutableArray<InputDevice> updatedDevices = default;

                            ImmutableInterlocked.Update(ref _inputDevices, (devices, device) =>
                            {
                                updatedDevices = devices.Remove(device);
                                return updatedDevices;
                            }, device);

                            // use the saved array here to invoke the event.
                            // this ensures that the event is invoked with the correct state.
                            InputDevicesChanged?.Invoke(this, updatedDevices);
                        };

                        deviceBuilder.Add(device);
                    }
                }

                // atomically update InputDevices
                var oldDevices = _inputDevices;
                _inputDevices = deviceBuilder.ToImmutable();
                DisposeDevices(oldDevices);
                InputDevicesChanged?.Invoke(this, InputDevices);

                if (!InputDevices.Any())
                {
                    Log.Write("Detect", "No tablets were detected.");
                }
            }
        }

        private bool TryMatch(IDeviceEndpoint device, TabletConfiguration configuration, List<DeviceIdentifier> identifiers, [NotNullWhen(true)] out InputDeviceEndpoint? endpoint)
        {
            foreach (var identifier in identifiers)
            {
                var match = device.VendorID == identifier.VendorID &&
                    device.ProductID == identifier.ProductID &&
                    device.CanOpen &&
                    (identifier.InputReportLength == null || identifier.InputReportLength == device.InputReportLength) &&
                    (identifier.OutputReportLength == null || identifier.OutputReportLength == device.OutputReportLength) &&
                    DeviceMatchesStrings(device, identifier.DeviceStrings) &&
                    DeviceMatchesAttribute(device, configuration.Attributes);

                if (match)
                {
                    endpoint = new InputDeviceEndpoint(this, device, configuration, identifier);
                    return true;
                }
            }

            endpoint = null;
            return false;
        }

        private static bool DeviceMatchesStrings(IDeviceEndpoint device, IDictionary<byte, string>? deviceStrings)
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
                    Log.Exception(ex, LogLevel.Debug);
                    return false;
                }
            }
            return true;
        }

        private static bool DeviceMatchesAttribute(IDeviceEndpoint device, Dictionary<string, string> attributes)
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
                case SystemPlatform.Linux:
                {
                    var devName = device.DevicePath;
                    var match = Regex.Match(devName, @"^.*\/.*?:.*?\.(?<interface>.+)\/.*?\/hidraw\/hidraw\d+$");
                    bool interfaceMatches = !attributes.ContainsKey("LinuxInterface") || match.Groups["interface"].Value == attributes["LinuxInterface"];

                    return interfaceMatches;
                }
                default:
                {
                    return true;
                }
            }
        }

        private static Dictionary<(int, int), List<TabletConfiguration>> ConstructConfigHashMap(ImmutableArray<TabletConfiguration> configs)
        {
            var map = new Dictionary<(int, int), List<TabletConfiguration>>();

            foreach (var config in configs)
            {
                foreach (var identifier in config.DigitizerIdentifiers)
                {
                    var key = (identifier.VendorID, identifier.ProductID);

                    ref var list = ref CollectionsMarshal.GetValueRefOrAddDefault(map, key, out var exists);
                    if (!exists)
                        list = new List<TabletConfiguration>();

                    if (!list!.Contains(config))
                        list.Add(config);
                }
            }

            return map;
        }

        private static void DisposeDevices(IEnumerable<InputDevice> devices)
        {
            foreach (var device in devices)
                device.Dispose();
        }

        public void Dispose()
        {
            DisposeDevices(InputDevices);
            GC.SuppressFinalize(this);
        }

        private class InputDeviceEndpointPair
        {
            public InputDeviceEndpoint? Digitizer { get; set; }
            public InputDeviceEndpoint? Auxiliary { get; set; }
        }
    }
}
