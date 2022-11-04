using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using OpenTabletDriver.Interop;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Components;
using OpenTabletDriver.Plugin.Devices;
using OpenTabletDriver.Plugin.Tablet;

#nullable enable

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

        public IReportParser<IDeviceReport> GetReportParser(DeviceIdentifier identifier)
        {
            return _reportParserProvider.GetReportParser(identifier.ReportParser);
        }

        public virtual bool Detect()
        {
            lock (_detectSync)
            {
                bool success = false;

                Log.Write("Detect", "Searching for tablets...");

                var treeBuilder = ImmutableArray.CreateBuilder<InputDeviceTree>();
                foreach (var config in _deviceConfigurationProvider.TabletConfigurations)
                {
                    if (Match(config) is InputDeviceTree tree)
                    {
                        success = true;
                        treeBuilder.Add(tree);

                        tree.Disconnected += (sender, e) =>
                        {
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

        protected virtual InputDeviceTree? Match(TabletConfiguration config)
        {
            Log.Debug("Detect", $"Searching for tablet '{config.Name}'");
            try
            {
                var devices = new List<InputDevice>();
                if (MatchDevice(config, config.DigitizerIdentifiers) is InputDevice digitizer)
                {
                    Log.Write("Detect", $"Found tablet '{config.Name}'");
                    devices.Add(digitizer);

                    if (config.AuxilaryDeviceIdentifiers.Any())
                    {
                        if (MatchDevice(config, config.AuxilaryDeviceIdentifiers) is InputDevice aux)
                            devices.Add(aux);
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

        private InputDevice? MatchDevice(TabletConfiguration config, IList<DeviceIdentifier> identifiers)
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
                        return new InputDevice(this, dev, config, identifier);
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
            return from device in CompositeDeviceHub.GetDevices()
                   where identifier.VendorID == device.VendorID
                   where identifier.ProductID == device.ProductID
                   where device.CanOpen
                   where identifier.InputReportLength == null || identifier.InputReportLength == device.InputReportLength
                   where identifier.OutputReportLength == null || identifier.OutputReportLength == device.OutputReportLength
                   where DeviceMatchesStrings(device, identifier.DeviceStrings)
                   where DeviceMatchesAttribute(device, configuration.Attributes)
                   select device;
        }

        private bool DeviceMatchesStrings(IDeviceEndpoint device, IDictionary<byte, string> deviceStrings)
        {
            if (deviceStrings == null || deviceStrings.Count == 0)
                return true;

            foreach (var matchQuery in deviceStrings)
            {
                try
                {
                    // Iterate through each device string, if one doesn't match then its the wrong configuration.
                    var input = device.GetDeviceString(matchQuery.Key);
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
                case PluginPlatform.Windows:
                {
                    var devName = device.DevicePath;

                    bool interfaceMatches = attributes.ContainsKey("WinInterface") ? Regex.IsMatch(devName, $"&mi_{attributes["WinInterface"]}") : true;
                    bool keyMatches = attributes.ContainsKey("WinUsage") ? Regex.IsMatch(devName, $"&col{attributes["WinUsage"]}") : true;

                    return interfaceMatches && keyMatches;
                }
                case PluginPlatform.MacOS:
                {
                    var devName = device.DevicePath;
                    bool interfaceMatches = attributes.ContainsKey("MacInterface") ? Regex.IsMatch(devName, $"IOUSBHostInterface@{attributes["MacInterface"]}") : true;
                    return interfaceMatches;
                }
                default:
                {
                    return true;
                }
            }
        }

        private static void DisposeDevices(ImmutableArray<InputDeviceTree> trees)
        {
            foreach (var tree in trees)
                foreach (var device in tree.InputDevices.ToArray())
                    device.Dispose();
        }

        public void Dispose()
        {
            DisposeDevices(_inputDeviceTrees);
        }
    }
}
