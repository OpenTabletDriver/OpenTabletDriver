using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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

        public event EventHandler<IEnumerable<TabletReference>>? TabletsChanged;

        public ICompositeDeviceHub CompositeDeviceHub { get; }
        public IList<InputDeviceTree> InputDevices { get; } = new List<InputDeviceTree>();
        public IEnumerable<TabletReference> Tablets => InputDevices.Select(c => c.CreateReference());

        public IReportParser<IDeviceReport> GetReportParser(DeviceIdentifier identifier)
        {
            return _reportParserProvider.GetReportParser(identifier.ReportParser);
        }

        public virtual bool Detect()
        {
            bool success = false;

            InputDevices.Clear();
            foreach (var config in _deviceConfigurationProvider.TabletConfigurations)
            {
                if (Match(config) is InputDeviceTree tree)
                {
                    success = true;
                    InputDevices.Add(tree);

                    tree.Disconnected += (sender, e) =>
                    {
                        InputDevices.Remove(tree);
                        TabletsChanged?.Invoke(this, Tablets);
                    };
                }
            }

            TabletsChanged?.Invoke(this, Tablets);

            return success;
        }

        protected virtual InputDeviceTree? Match(TabletConfiguration config)
        {
            Log.Write("Detect", $"Searching for tablet '{config.Name}'");
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
                    "Follow the instructions from https://github.com/OpenTabletDriver/OpenTabletDriver/wiki/Linux-FAQ#the-driver-fails-to-open-the-tablet-deviceioexception to resolve this issue.",
                    LogLevel.Error
                );
            }
            catch (ArgumentOutOfRangeException aex) when (aex.Message.Contains("Value range is [0, 15]")
                && SystemInterop.CurrentPlatform == PluginPlatform.Linux)
            {
                Log.Write(
                    "Driver",
                    "Device is currently in use by another kernel module. " +
                    "Follow the instructions from https://github.com/OpenTabletDriver/OpenTabletDriver/wiki/Linux-FAQ#argumentoutofrangeexception-value-0-15 to resolve this issue.",
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
                default:
                {
                    return true;
                }
            }
        }

        public void Dispose()
        {
            foreach (InputDeviceTree tree in InputDevices)
                foreach (InputDevice dev in tree.InputDevices)
                    dev.Dispose();
        }
    }
}
