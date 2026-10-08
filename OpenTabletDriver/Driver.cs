using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using OpenTabletDriver.Devices.HidSharpBackend;
using OpenTabletDriver.Interop;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Components;
using OpenTabletDriver.Plugin.Devices;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver
{
    public class Driver : IDriver, IDisposable
    {
        public Driver(ICompositeDeviceHub deviceHub, IReportParserProvider reportParserProvider, IDeviceConfigurationProvider configurationProvider)
        {
            CompositeDeviceHub = deviceHub;
            _reportParserProvider = reportParserProvider;
            _deviceConfigurationProvider = configurationProvider;
        }

        private readonly IReportParserProvider _reportParserProvider;
        private readonly IDeviceConfigurationProvider _deviceConfigurationProvider;
        private readonly object _detectSync = new object();
        private ImmutableArray<InputDeviceTree> _inputDeviceTrees = ImmutableArray<InputDeviceTree>.Empty;

        public event EventHandler<IEnumerable<TabletReference>>? TabletsChanged;

        public ICompositeDeviceHub CompositeDeviceHub { get; }
        public ImmutableArray<InputDeviceTree> InputDevices => _inputDeviceTrees;
        public IEnumerable<TabletReference> Tablets => InputDevices.Select(c => c.CreateReference());

        private Dictionary<IDeviceEndpoint, Dictionary<byte, string?>> DeviceStringCache = [];

        public IReportParser<IDeviceReport> GetReportParser(DeviceIdentifier identifier)
        {
            return _reportParserProvider.GetReportParser(identifier.ReportParser);
        }

        public IEnumerable<int> KnownVendorIDs => (
            from configuration in _deviceConfigurationProvider.TabletConfigurations
            from identifier in configuration.DigitizerIdentifiers.Concat(configuration.AuxiliaryDeviceIdentifiers ??
                                                                         Enumerable.Empty<DeviceIdentifier>())
            select identifier.VendorID).Distinct();

        public virtual bool Detect()
        {
            lock (_detectSync)
            {
                DeviceStringCache = []; // reset cache on redetect

                bool success = false;

                Log.Write("Detect", "Searching for tablets...");
                var devicesEnumerable = CompositeDeviceHub.GetDevices();
                var devices = devicesEnumerable as IDeviceEndpoint[] ?? [.. devicesEnumerable];

                var treeBuilder = ImmutableArray.CreateBuilder<InputDeviceTree>();
                foreach (var config in _deviceConfigurationProvider.TabletConfigurations)
                {
                    if (Match(config, devices) is InputDeviceTree tree)
                    {
                        success = true;
                        treeBuilder.Add(tree);

                        tree.Disconnected += (sender, e) =>
                        {
                            tree.OutputMode?.Dispose();

                            // save the immutable array for later use
                            Unsafe.SkipInit(out ImmutableArray<InputDeviceTree> updatedTrees);
                            ImmutableInterlocked.Update(ref _inputDeviceTrees, (trees, tree) =>
                            {
                                updatedTrees = trees.Remove(tree);
                                return updatedTrees;
                            }, tree);

                            // use here, we do this to avoid using an _inputDeviceTrees that may have been updated
                            TabletsChanged?.Invoke(this, updatedTrees.Select(c => c.CreateReference()));
                        };
                    }
                }

                // atomically update InputDevices
                var oldDevices = _inputDeviceTrees;
                _inputDeviceTrees = treeBuilder.ToImmutable();
                DisposeDevices(oldDevices);
                TabletsChanged?.Invoke(this, Tablets);

                if (!success)
                {
                    Log.Write("Detect", "No tablets were detected.");
                }

                return success;
            }
        }

        protected virtual InputDeviceTree? Match(TabletConfiguration config, IEnumerable<IDeviceEndpoint> deviceHubDevices)
        {
            Log.Debug("Detect", $"Searching for tablet '{config.Name}'");
            try
            {
                var allDevices = deviceHubDevices as IDeviceEndpoint[] ?? [.. deviceHubDevices];

                var devices = new List<InputDevice>();

                var matchedDevices = MatchDevice(config, config.DigitizerIdentifiers, allDevices, DeviceStringCache);

                foreach (var (identifier, endpoints) in matchedDevices)
                {
                    try
                    {
                        devices.Add(new InputDevice(this, endpoints.First(), config, identifier));
                    }
                    catch (Exception e)
                    {
                        Log.Exception(e);
                        continue; // other identifiers may work, let's try those
                    }

                    Log.Write("Detect", $"Found tablet '{config.Name}'");

                    if ((config.AuxiliaryDeviceIdentifiers?.Count ?? 0) > 0)
                    {
                        var matchedAuxDevices = MatchDevice(config, config.AuxiliaryDeviceIdentifiers!, allDevices, DeviceStringCache);
                        bool configuredAuxDevice = false;
                        foreach (var (auxIdentifier, auxEndpoints) in matchedAuxDevices)
                        {
                            try
                            {
                                devices.Add(new InputDevice(this, auxEndpoints.First(), config, auxIdentifier));
                                configuredAuxDevice = true;
                                break;
                            }
                            catch (Exception e)
                            {
                                Log.Exception(e);
                            }
                        }
                        if (configuredAuxDevice)
                            Log.Debug("Detect", "Successfully found auxiliary device");
                        else
                            Log.Write("Detect", "Failed to find auxiliary device, express keys may be unavailable.", LogLevel.Warning);
                    }

                    return new InputDeviceTree(config, devices);
                }
            }
            catch (IOException iex) when (iex.Message.Contains("Unable to open HID class device")
                && SystemInterop.CurrentPlatform == PluginPlatform.Linux)
            {
                Log.Write(
                    "Driver",
                    "The current user does not have the permissions to open the device stream. " +
                    "Follow the instructions from https://opentabletdriver.net/Wiki/FAQ/Linux#fail-device-streams to resolve this issue.",
                    LogLevel.Error
                );
            }
            catch (ArgumentOutOfRangeException aex) when (aex.Message.Contains("Value range is [0, 15]")
                && SystemInterop.CurrentPlatform == PluginPlatform.Linux)
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

        public static IEnumerable<(DeviceIdentifier identifier, IDeviceEndpoint[] matches)> MatchDevice(
            TabletConfiguration config,
            IReadOnlyCollection<DeviceIdentifier> identifiers,
            IEnumerable<IDeviceEndpoint> deviceEndpoints,
            Dictionary<IDeviceEndpoint, Dictionary<byte, string?>>? deviceStringCache,
            [CallerArgumentExpression("identifiers")] string? identifierName = null)
        {
            var deviceEndpointsArray = deviceEndpoints as IDeviceEndpoint[] ?? [.. deviceEndpoints];
            foreach (var identifier in identifiers)
            {
                var matches = GetMatchingDevices(config, identifier, deviceEndpointsArray, deviceStringCache);

                switch (matches.Length)
                {
                    case 0:
                        continue;
                    case > 1:
                        Log.Write(config.Name, "More than 1 matching device has been found.", LogLevel.Warning);
                        goto case 1;
                    case 1:
                        Log.Debug("Detect", $"Identified '{config.Name}' on {identifierName}");
                        yield return (identifier, matches);
                        break;
                }
            }
        }

        private static IDeviceEndpoint[] GetMatchingDevices(TabletConfiguration configuration,
            DeviceIdentifier identifier, IEnumerable<IDeviceEndpoint> deviceEndpoints,
            Dictionary<IDeviceEndpoint, Dictionary<byte, string?>>? deviceStringCache)
        {
            return [.. from device in deviceEndpoints
                   where identifier.VendorID == device.VendorID
                   where identifier.ProductID == device.ProductID
                   where device.CanOpen
                   where identifier.InputReportLength == null || identifier.InputReportLength == device.InputReportLength
                   where identifier.OutputReportLength == null || identifier.OutputReportLength == device.OutputReportLength
                   where identifier.FeatureReportLength == null || identifier.FeatureReportLength == device.FeatureReportLength
                   where DeviceMatchesStrings(device, identifier.DeviceStrings, deviceStringCache)
                   where DeviceMatchesAttribute(device, identifier.Attributes, configuration.Attributes)
                   select device];
        }

        private static bool DeviceMatchesStrings(IDeviceEndpoint device, Dictionary<byte, string>? deviceStrings, Dictionary<IDeviceEndpoint, Dictionary<byte, string?>>? stringCache)
        {
            if (deviceStrings == null || deviceStrings.Count == 0)
                return true;

            // Iterate through each device string, if one doesn't match then its the wrong configuration.
            foreach (var matchQuery in deviceStrings)
            {
                try
                {
                    string? deviceString = null;
                    if (stringCache != null
                        && stringCache.TryGetValue(device, out var deviceCachedStrings)
                        && deviceCachedStrings.TryGetValue(matchQuery.Key, out var cacheDeviceString))
                    {
                        if (cacheDeviceString == null)
                        {
                            Log.Write("Detect", $"Cached null for index {matchQuery.Key}, skipping", LogLevel.Debug);
                            return false;
                        }
                        deviceString = cacheDeviceString;
                    }

                    deviceString ??= device.GetDeviceString(matchQuery.Key);

                    if (stringCache != null && !stringCache.TryAdd(device, new Dictionary<byte, string?> { { matchQuery.Key, deviceString } }))
                        stringCache[device].TryAdd(matchQuery.Key, deviceString);

                    // nullcheck after cache update to ensure nulls are cached
                    if (deviceString == null)
                        throw new IOException($"Unable to look up string index {matchQuery.Key}");

                    var pattern = matchQuery.Value;
                    if (!Regex.IsMatch(deviceString, pattern))
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

        private static bool DeviceMatchesAttribute(IDeviceEndpoint device, Dictionary<string, string>? identifier_attributes, Dictionary<string, string>? config_attributes)
        {
            var attributes = new Dictionary<string, string>(identifier_attributes ?? Enumerable.Empty<KeyValuePair<string, string>>());

            if (config_attributes != null)
            {
                foreach (var kvp in config_attributes)
                    attributes.TryAdd(kvp.Key, kvp.Value);
            }

            // Windows only configuration attribute.
            if (SystemInterop.CurrentPlatform == PluginPlatform.Windows)
            {
                if (device is HidSharpEndpoint
                    && attributes.TryGetValue("WinUsage", out var winUsage)
                    && !Regex.IsMatch(device.DevicePath, $"&col{winUsage}"))
                {
                    // If it isn't a match there is no point proceeding.
                    return false;
                }
            }

            if (attributes.TryGetValue("HidReports", out var hidReports)
                && device.DeviceAttributes.TryGetValue("HID_REPORTS", out var usbHidReports)
                && !Regex.IsMatch(usbHidReports, hidReports))
            {
                return false; // HidReports specified and no HID Reports match.
            }

            if (!attributes.TryGetValue("Interface", out var identifierInterface))
                return true; // No interface specified, match.

            if (!device.DeviceAttributes.TryGetValue("USB_INTERFACE_NUMBER", out var usbInterface))
                return false; // Device doesn't have an interface number, not a match.

            return identifierInterface == usbInterface;
        }

        private static void DisposeDevices(ImmutableArray<InputDeviceTree> trees)
        {
            foreach (var tree in trees)
                foreach (var device in tree.InputDevices.ToArray())
                    device.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _isDisposed;

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                DisposeDevices(_inputDeviceTrees);
            }

            _isDisposed = true;
        }
    }
}
