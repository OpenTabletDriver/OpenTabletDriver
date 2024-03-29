using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
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
                    ScanDevices();
                };
            }

            _configHashMap = ConstructConfigHashMap(_deviceConfigurationProvider.TabletConfigurations);
            _compositeDeviceHub.DevicesChanged += (sender, d) =>
            {
                var additions = d.Additions.ToArray();
                if (additions.Length == 0)
                    return;

                var addedDevices = ScanDevices(additions);
                if (addedDevices.Length == 0)
                    return;

                ImmutableInterlocked.Update(ref _inputDevices, devices => devices.AddRange(addedDevices));
                InputDevice.AssignPersistentId(_inputDevices);

                foreach (var device in addedDevices)
                {
                    device.Initialize(true);
                    OnInputDeviceAdded(this, device);
                }
            };
        }

        public event EventHandler<InputDevice>? InputDeviceAdded;
        public event EventHandler<InputDevice>? InputDeviceRemoved;

        protected virtual void OnInputDeviceAdded(object? sender, InputDevice device)
        {
            InputDeviceAdded?.Invoke(sender, device);
        }

        protected virtual void OnInputDeviceRemoved(object? sender, InputDevice device)
        {
            InputDeviceRemoved?.Invoke(sender, device);
        }

        public ImmutableArray<InputDevice> InputDevices => _inputDevices;

        public IReportParser<IDeviceReport> GetReportParser(DeviceIdentifier identifier)
        {
            return _reportParserProvider.GetReportParser(identifier.ReportParser);
        }

        public void ScanDevices()
        {
            Log.Write("Detect", "Searching for tablets...");

            DisposeDevices(_inputDevices);
            _inputDevices = ScanDevices(_compositeDeviceHub.GetDevices().ToArray());
            InputDevice.AssignPersistentId(_inputDevices);

            if (InputDevices.Any())
                Log.Write("Detect", "Search done");
            else
                Log.Write("Detect", "Search done. No tablets were detected.");

            foreach (var device in _inputDevices)
            {
                device.Initialize(true);
                OnInputDeviceAdded(this, device);
            }
        }

        /// <summary>
        /// Returns an array of uninitialized <see cref="InputDevice"/>s from given <see cref="IDeviceEndpoint"/>s.
        /// See <see cref="InputDevice.Initialize(bool)"/> for initialization.
        /// </summary>
        /// <param name="endpointsToScan">The endpoints to scan for</param>
        /// <returns>An array of uninitialized <see cref="InputDevice"/>s.</returns>
        private ImmutableArray<InputDevice> ScanDevices(IDeviceEndpoint[] endpointsToScan)
        {
            lock (_detectLock)
            {
                // save a reference to avoid potential _configHashMap changes during detection
                var tabletConfigurations = _configHashMap;

                // group device endpoints of the same device
                var deviceEndpointGroups = new List<List<IDeviceEndpoint>>();

                foreach (var endpoint in endpointsToScan)
                {
                    static bool isGroup(IDeviceEndpoint a, IDeviceEndpoint b)
                    {
                        return a.VendorID == b.VendorID
                            && a.ProductID == b.ProductID
                            && a.IsSibling(b);
                    }

                    var group = deviceEndpointGroups.FirstOrDefault(g => isGroup(endpoint, g[0]));

                    if (group is null)
                    {
                        group = new List<IDeviceEndpoint>();
                        deviceEndpointGroups.Add(group);
                    }

                    group.Add(endpoint);
                }

                var inputDevices = new List<InputDevice>();
                foreach (var device in deviceEndpointGroups)
                {
                    var deviceHash = (device[0].VendorID, device[0].ProductID);
                    var deviceName = device[0].FriendlyName;

                    // loop over configurations that has an identifier that matches the device's VID/PID
                    if (!tabletConfigurations.TryGetValue(deviceHash, out var candidateConfigs))
                        continue;

                    TabletConfiguration? tabletConfiguration = null;
                    InputDeviceEndpoint? digitizerEndpoint = null;
                    InputDeviceEndpoint? auxiliaryEndpoint = null;

                    var enumerator = device.GetEnumerator();

                    // Find matching configuration
                    while (enumerator.MoveNext())
                    {
                        var endpoint = enumerator.Current;

                        foreach (var candidateConfig in candidateConfigs)
                        {
                            if (TryMatch(endpoint, candidateConfig, candidateConfig.DigitizerIdentifiers, out digitizerEndpoint))
                            {
                                tabletConfiguration = candidateConfig;
                                if (candidateConfig.AuxiliaryDeviceIdentifiers.Count == 0)
                                    goto build_device;

                                goto found_config;
                            }

                            if (TryMatch(endpoint, candidateConfig, candidateConfig.AuxiliaryDeviceIdentifiers, out auxiliaryEndpoint))
                            {
                                tabletConfiguration = candidateConfig;
                                goto found_config;
                            }
                        }
                    }

                    // no matching configuration found, move on to the next device
                    continue;

                found_config:

                    // we found a matching configuration, now search for the other endpoint
                    while ((digitizerEndpoint is null || auxiliaryEndpoint is null) && enumerator.MoveNext())
                    {
                        var endpoint = enumerator.Current;

                        if (digitizerEndpoint is null && TryMatch(endpoint, tabletConfiguration!, tabletConfiguration!.DigitizerIdentifiers, out digitizerEndpoint))
                            continue;

                        if (auxiliaryEndpoint is null && TryMatch(endpoint, tabletConfiguration!, tabletConfiguration!.AuxiliaryDeviceIdentifiers, out auxiliaryEndpoint))
                            continue;
                    }

                    if (digitizerEndpoint is null)
                    {
                        Log.Write("Detect", $"'{tabletConfiguration!.Name}' digitizer not found", LogLevel.Warning);
                        continue;
                    }

                build_device:

                    // whole log should be Warning when auxiliary is unexpectedly null
                    var warning = tabletConfiguration!.AuxiliaryDeviceIdentifiers.Count > 0 && auxiliaryEndpoint is null
                        ? LogLevel.Warning
                        : LogLevel.Info;

                    Log.Write("Detect", $"{tabletConfiguration.Name} detected", warning);
                    Log.Write("Detect", $"{tabletConfiguration.Name} digitizer: {digitizerEndpoint.Endpoint.DevicePath}", warning);

                    if (auxiliaryEndpoint is not null)
                        Log.Write("Detect", $"{tabletConfiguration.Name} auxiliary: {auxiliaryEndpoint.Endpoint.DevicePath}", warning);
                    else if (tabletConfiguration!.AuxiliaryDeviceIdentifiers.Count > 0) // aux is null
                        Log.Write("Detect", $"{tabletConfiguration.Name} auxiliary: N/A, express keys on tablet may not work", warning);

                    var inputDevice = new InputDevice(tabletConfiguration!, digitizerEndpoint, auxiliaryEndpoint);

                    inputDevice.StateChanged += (sender, e) =>
                    {
                        if (e < InputDeviceState.Disconnected)
                            return;

                        if (ImmutableInterlocked.Update(ref _inputDevices, (devices, device) => devices.Remove(device), inputDevice))
                            OnInputDeviceRemoved(this, inputDevice);

                        if (e == InputDeviceState.Disconnected)
                            Log.Write("Detect", $"{inputDevice.PersistentName} disconnected", LogLevel.Info);
                    };

                    inputDevices.Add(inputDevice);
                }

                return inputDevices.ToImmutableArray();
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
            {
                device.Dispose();
            }
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
