using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using HidSharp;
using OpenTabletDriver.Devices;
using OpenTabletDriver.Interop;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver
{
    public class Driver : IDriver, IDisposable
    {
        public Driver()
        {
            Info.GetDriverInstance = () => this;

            DeviceList.Local.Changed += (sender, e) =>
            {
                var newList = DeviceList.Local.GetHidDevices();
                var changes = new DevicesChangedEventArgs(CurrentDevices, newList);
                if (changes.Any())
                {
                    DevicesChanged?.Invoke(this, changes);
                    CurrentDevices = newList;
                }
            };

            TabletHandlerCreated += (_, id) =>
            {
                Log.Debug("Detect", $"{id} has been created");
            };

            TabletHandlerDestroyed += (_, id) =>
            {
                Log.Debug("Detect", $"{id} has been destroyed");
            };
        }

        protected IEnumerable<HidDevice> CurrentDevices { set; get; } = DeviceList.Local.GetHidDevices();

        private readonly TabletHandlerStore TabletHandlerStore = new TabletHandlerStore();

        public event EventHandler<TabletHandlerID> TabletHandlerCreated;
        public event EventHandler<TabletHandlerID> TabletHandlerDestroyed;
        public event EventHandler<DevicesChangedEventArgs> DevicesChanged;

        public void EnumerateTablets(IEnumerable<TabletConfiguration> tabletConfigurations)
        {
            foreach (var id in GetActiveTabletHandlerIDs())
            {
                TabletHandlerDestroyed?.Invoke(this, id);
                TabletHandlerStore.Remove(id);
            }

            ProcessDevices(DeviceList.Local.GetHidDevices(), tabletConfigurations);
        }

        public void ProcessDevices(IEnumerable<object> device, IEnumerable<TabletConfiguration> tabletConfigurations)
        {
            if (device.FirstOrDefault() is not HidDevice)
                return;

            // Group configuration by their VendorID then cache results to an array
            var groupedConfigs = tabletConfigurations.GroupBy(config => config.CompatibleVendorID).ToDictionary(c => c.Key);
            var availableVendorIDs = groupedConfigs.Select(group => group.Key).ToArray();

            var hidDevices = device.Select(d => (HidDevice)d);

            // Group HIDs by their VID:PID before iterating
            foreach (var deviceGroup in hidDevices.OrderBy(h => h.DevicePath).GroupBy(hid => (hid.VendorID, hid.ProductID)))
            {
                var vidpid = $"{deviceGroup.Key.VendorID}:{deviceGroup.Key.ProductID}";

                Log.Debug("Detect", $"Processing device/s: {vidpid}");

                // Skip if device VendorID is not supported
                if (!availableVendorIDs.Contains(deviceGroup.Key.VendorID))
                    continue;

                Log.Write("Detect", $"Found compatible device: {vidpid}");

                // Retrieve matching configuration and identifiers for the device
                var compatibleConfigs = groupedConfigs[deviceGroup.Key.VendorID];

                PartialTabletMatch[] partialTabletMatch;

                try
                {
                    partialTabletMatch = GetMatchingConfiguration(deviceGroup, compatibleConfigs).ToArray();
                }
                catch (TabletEnumerationException e)
                {
                    // GetMatchingConfiguration() encountered known exceptions, stop enumeration
                    Log.Exception(e);
                    Log.Write("Detect", "Tablet enumeration stopped due to detection issues", LogLevel.Error);
                    break;
                }

                if (partialTabletMatch == null || !partialTabletMatch.Any())
                {
                    Log.Write("Detect", $"No matching configuration found for device: {tempname}", LogLevel.Warning);
                    LogPossibleWinUSBInterference(deviceGroup.Key.VendorID);
                    continue;
                }

                var tabletDigitizers = partialTabletMatch
                    .Select(p => new TabletMatch<DeviceIdentifier>(p.TabletConfiguration, p.Device, p.DigitizerIdentifiers))
                    .Where(m => m.Identifiers.Length > 0).ToArray();

                var tabletAuxilary = partialTabletMatch
                    .Select(p => new TabletMatch<DeviceIdentifier>(p.TabletConfiguration, p.Device, p.AuxilaryIdentifiers))
                    .Where(m => m.Identifiers.Length > 0).ToArray();

                if (!tabletDigitizers.Any())
                {
                    Log.Write("Detect", $"Failed to find digitizer for device: {tempname}", LogLevel.Warning);
                    LogPossibleWinUSBInterference(deviceGroup.Key.VendorID);
                    continue;
                }

                // An unlikely scenario, but in the case that we detected an unbalanced count for auxilary and digitizer
                // we'll disable the auxilary as we have no way yet of knowing which auxilary resides within the same
                // USB device as the digitizer.
                //
                // This is a limitation of HidSharpCore
                if (tabletAuxilary.Any() && tabletAuxilary.Length != tabletDigitizers.Length)
                {
                    Log.Write("Detect", $"Unbalanced amount of digitizers ({tabletDigitizers.Length}) and auxilaries ({tabletAuxilary.Length}) detected, " +
                        "auxilary feature will be disabled", LogLevel.Warning);
                    tabletAuxilary = Array.Empty<TabletMatch<DeviceIdentifier>>();
                }

                if (tabletAuxilary.Any())
                {
                    foreach ((var digitizer, var auxilary) in tabletDigitizers.Zip(tabletAuxilary))
                    {
                        var tabletHandler = CreateTabletHandler(digitizer, auxilary);
                        TabletHandlerStore.Add(tabletHandler);
                        TabletHandlerCreated?.Invoke(this, tabletHandler.InstanceID);
                    }
                }
                else
                {
                    foreach (var digitizer in tabletDigitizers)
                    {
                        var tabletHandler = CreateTabletHandler(digitizer);
                        TabletHandlerStore.Add(tabletHandler);
                        TabletHandlerCreated?.Invoke(this, tabletHandler.InstanceID);
                    }
                }
            }

            if (!TabletHandlerStore.GetTabletHandlers().Any())
            {
                Log.Write("Detect", "No tablet detected.");
            }
        }

        public TabletHandler GetTabletHandler(TabletHandlerID ID)
        {
            return TabletHandlerStore[ID];
        }

        public IEnumerable<TabletHandler> GetTabletHandlers()
        {
            return TabletHandlerStore.GetTabletHandlers();
        }

        public IEnumerable<TabletHandlerID> GetActiveTabletHandlerIDs()
        {
            return TabletHandlerStore.GetTabletHandlers().Select(t => t.InstanceID);
        }

        public void SetOutputMode(TabletHandlerID ID, IOutputMode outputMode)
        {
            GetTabletHandler(ID).OutputMode = outputMode;
        }

        public IOutputMode GetOutputMode(TabletHandlerID ID)
        {
            return GetTabletHandler(ID)?.OutputMode;
        }

        public TabletState GetTabletState(TabletHandlerID ID)
        {
            return GetTabletHandler(ID)?.TabletState;
        }

        private TabletHandler CreateTabletHandler(TabletMatch<DeviceIdentifier> digitizer)
        {
            if (digitizer.Identifiers.Length > 1)
                Log.Write("Detect", "More than one matching digitizer identifier found, will ignore other identifiers", LogLevel.Warning);

            var tabletDigitizerIdentifier = digitizer.Identifiers[0];
            var config = digitizer.Configuration;

            Log.Write("Detect", $"Using device identified as '{config.Name}'");

            var tabletHandler = new TabletHandler(digitizer.Device, null)
            {
                TabletState = new TabletState(config, tabletDigitizerIdentifier, null)
            };

            tabletHandler.Disconnected += (sender, id) =>
            {
                TabletHandlerDestroyed?.Invoke(this, id);
                TabletHandlerStore.Remove(id);
            };

            tabletHandler.Initialize();

            return tabletHandler;
        }

        private TabletHandler CreateTabletHandler(TabletMatch<DeviceIdentifier> digitizer, TabletMatch<DeviceIdentifier> auxilary)
        {
            var uniqueAux = GetMatchingUniqueAuxilaryIdentifiers(digitizer.Identifiers, auxilary.Identifiers);
            if (digitizer.Identifiers.Length > 1)
                Log.Write("Detect", "More than one matching digitizer identifier found, will ignore other identifiers", LogLevel.Warning);
            if (auxilary.Identifiers.Length > 1)
                Log.Write("Detect", "More than one matching auxilary identifier found, will ignore other identifiers", LogLevel.Warning);

            var tabletDigitizerIdentifier = digitizer.Identifiers[0];
            var tabletAuxilaryIdentifier = uniqueAux.FirstOrDefault();
            var config = digitizer.Configuration;

            Log.Write("Detect", $"Using device identified as '{config.Name}'");

            var tabletHandler = new TabletHandler(digitizer.Device, auxilary.Device)
            {
                TabletState = new TabletState(config, tabletDigitizerIdentifier, tabletAuxilaryIdentifier)
            };

            tabletHandler.Disconnected += (sender, id) =>
            {
                TabletHandlerDestroyed?.Invoke(this, id);
                TabletHandlerStore.Remove(id);
            };

            tabletHandler.Initialize();

            return tabletHandler;
        }

        private static IEnumerable<PartialTabletMatch> GetMatchingConfiguration(IEnumerable<HidDevice> devices, IGrouping<int, TabletConfiguration> configurations)
        {
            foreach (var device in devices.Where(d => SafeGetCanOpen(d)))
            {
                Log.Write("Detect", $"Searching matching configuration for path: {device.DevicePath}");
                foreach (var config in configurations)
                {
                    var digitizerIdentifiers = Enumerable.Empty<DeviceIdentifier>();
                    var auxilaryIdentifiers = Enumerable.Empty<DeviceIdentifier>();

                    Log.Write("Detect", $"Trying to match configuration: {config.Name}");

                    try
                    {
                        digitizerIdentifiers = GetMatchingIdentifiers(device, config.DigitizerIdentifiers) ?? Enumerable.Empty<DeviceIdentifier>();
                        auxilaryIdentifiers = GetMatchingIdentifiers(device, config.AuxilaryDeviceIdentifiers) ?? Enumerable.Empty<DeviceIdentifier>();
                    }
                    catch (IOException iex) when (iex.Message.Contains("Unable to open HID class device")
                        && SystemInterop.CurrentPlatform == PluginPlatform.Linux)
                    {
                        throw new TabletEnumerationException("Current user don't have the permissions to open device streams. "
                            + "To fix this issue, please follow the instructions from https://github.com/OpenTabletDriver/OpenTabletDriver/wiki/Linux-FAQ#the-driver-fails-to-open-the-tablet-deviceioexception");
                    }
                    catch (ArgumentOutOfRangeException aex) when (aex.Message.Contains("Value range is [0, 15]")
                        && SystemInterop.CurrentPlatform == PluginPlatform.Linux)
                    {
                        throw new TabletEnumerationException("Device is currently in use by another kernel module. "
                            + "To fix this issue, please follow the instructions from https://github.com/OpenTabletDriver/OpenTabletDriver/wiki/Linux-FAQ#argumentoutofrangeexception-value-0-15");
                    }
                    catch (Exception ex)
                    {
                        // If exception is not known, log error then continue enumeration
                        Log.Exception(ex);
                    }

                    if (digitizerIdentifiers.Any() || auxilaryIdentifiers.Any())
                    {
                        yield return new PartialTabletMatch
                        {
                            Device = device,
                            TabletConfiguration = config,
                            DigitizerIdentifiers = digitizerIdentifiers.ToArray(),
                            AuxilaryIdentifiers = auxilaryIdentifiers.ToArray()
                        };

                        // Already found a matching config, break and continue to next device
                        break;
                    }
                }
            }
        }

        private static IEnumerable<T> GetMatchingIdentifiers<T>(HidDevice device, IEnumerable<T> deviceIdentifiers) where T : DeviceIdentifier
        {
            return deviceIdentifiers.Where(identifier =>
                identifier.ProductID == device.ProductID
                && (identifier.InputReportLength == null || identifier.InputReportLength == device.GetMaxInputReportLength())
                && (identifier.OutputReportLength == null || identifier.OutputReportLength == device.GetMaxOutputReportLength())
                && DeviceMatchesAllStrings(device, identifier)
            );
        }

        private static IEnumerable<DeviceIdentifier> GetMatchingUniqueAuxilaryIdentifiers(IEnumerable<DeviceIdentifier> digitizerIdentifiers, IEnumerable<DeviceIdentifier> auxilaryIdentifiers)
        {
            return auxilaryIdentifiers.Where(identifier =>
            {
                // Remove auxiliary identifiers that match the same device specified by digitizer identifiers
                var isConflicting = digitizerIdentifiers.Any(digitizerIdentifier =>
                    identifier.ProductID == digitizerIdentifier.ProductID
                    && (identifier.InputReportLength == null || digitizerIdentifier.InputReportLength == null
                    || identifier.InputReportLength == digitizerIdentifier.InputReportLength)
                    && (identifier.OutputReportLength == null || digitizerIdentifier.OutputReportLength == null
                    || identifier.OutputReportLength == digitizerIdentifier.OutputReportLength));

                if (isConflicting)
                    Log.Write("Detect", "Conflicting auxiliary identifier detected and removed", LogLevel.Warning);

                return !isConflicting;
            });
        }

        private static bool DeviceMatchesAllStrings(HidDevice device, DeviceIdentifier identifier)
        {
            if (identifier.DeviceStrings == null || identifier.DeviceStrings.Count == 0)
                return true;

            return identifier.DeviceStrings.All(matchQuery =>
            {
                try
                {
                    // Iterate through each device string, if one doesn't match then its the wrong configuration.
                    var input = device.GetDeviceString(matchQuery.Key);
                    var pattern = matchQuery.Value;
                    return Regex.IsMatch(input, pattern);
                }
                catch (Exception ex)
                {
                    Log.Exception(ex);
                    return false;
                }
            });
        }

        private static string SafeGetDeviceName(HidDevice device)
        {
            var defaultName = "Unnamed Device";
            try
            {
                return device.GetFriendlyName() ?? defaultName;
            }
            catch
            {
                Log.Exception(new UnauthorizedAccessException("Failed to access device name"));
                return defaultName;
            }
        }

        private static bool SafeGetCanOpen(HidDevice device)
        {
            try
            {
                return device.CanOpen;
            }
            catch
            {
                return false;
            }
        }

        private static void LogPossibleWinUSBInterference(int vendorID)
        {
            if (SystemInterop.CurrentPlatform == PluginPlatform.Windows && vendorID == 9580)
            {
                Log.Write("Detect", "Interference from WinUSB is likely. For a fix, please visit: https://github.com/OpenTabletDriver/OpenTabletDriver/wiki/Windows-FAQ#my-gaomonhuion-tablet-isnt-detected-but-is-listed-as-supported", LogLevel.Error);
            }
        }

        public void Dispose()
        {
            TabletHandlerStore.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
