using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using HidSharp;
using OpenTabletDriver.Devices;
using OpenTabletDriver.Interop;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Interpolator;
using OpenTabletDriver.Reflection;
using OpenTabletDriver.Tablet;

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
                DevicesChanged?.Invoke(this, new DevicesChangedEventArgs(CurrentDevices, newList));
                CurrentDevices = newList;
            };
        }
        
        public event EventHandler<bool> Reading;
        public event EventHandler<IDeviceReport> ReportRecieved;
        public event EventHandler<DevicesChangedEventArgs> DevicesChanged;
        public event EventHandler<TabletState> TabletChanged;

        protected IEnumerable<HidDevice> CurrentDevices { set; get; } = DeviceList.Local.GetHidDevices();

        protected virtual PluginManager PluginManager { get; } = new PluginManager();
        
        public bool EnableInput { set; get; }
        public bool InterpolatorActive => Interpolators.Any();

        private TabletState tablet;
        public TabletState Tablet
        {
            private set
            {
                // Stored locally to avoid re-detecting to switch output modes
                this.tablet = value;
                if (OutputMode != null)
                    OutputMode.Tablet = Tablet;
                TabletChanged?.Invoke(this, Tablet);
            }
            get => this.tablet;
        }

        public IOutputMode OutputMode { set; get; }
        
        public DeviceReader<IDeviceReport> TabletReader { private set; get; }
        public DeviceReader<IDeviceReport> AuxReader { private set; get; }

        public Collection<Interpolator> Interpolators { set; get; } = new Collection<Interpolator>();

        public bool TryMatch(TabletConfiguration config)
        {
            Log.Write("Detect", $"Searching for tablet '{config.Name}'");
            try
            {
                if (TryMatchDigitizer(config, out var digitizer))
                {
                    Log.Write("Detect", $"Found tablet '{config.Name}'");
                    if (!TryMatchAuxDevice(config, out var aux))
                    {
                        Log.Write("Detect", "Failed to find auxiliary device, express keys may be unavailable.", LogLevel.Warning);
                    }

                    Tablet = new TabletState(config, digitizer, aux);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
            Tablet = null;
            return false;
        }

        protected bool TryMatchDigitizer(TabletConfiguration config, out DigitizerIdentifier digitizerIdentifier)
        {
            digitizerIdentifier = default;
            foreach (var identifier in config.DigitizerIdentifiers)
            {
                var matches = FindMatchingDigitizer(identifier, config.Attributes);

                if (matches.Count() > 1)
                    Log.Write("Detect", "More than 1 matching digitizer has been found.", LogLevel.Warning);

                foreach (HidDevice dev in matches)
                {
                    // Try every matching device until we initialize successfully
                    try
                    {
                        var parser = GetReportParser(identifier.ReportParser) ?? new TabletReportParser();
                        InitializeDigitizerDevice(dev, identifier, parser);
                        digitizerIdentifier = identifier;
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Log.Exception(ex);
                        continue;
                    }
                }
            }
            return config.DigitizerIdentifiers.Count == 0;
        }

        protected bool TryMatchAuxDevice(TabletConfiguration config, out DeviceIdentifier auxIdentifier)
        {
            auxIdentifier = default;
            foreach (var identifier in config.AuxilaryDeviceIdentifiers)
            {
                var matches = FindMatchingAuxiliary(identifier, config.Attributes);

                if (matches.Count() > 1)
                    Log.Write("Detect", "More than 1 matching auxiliary device has been found.", LogLevel.Warning);

                foreach (HidDevice dev in matches)
                {
                    // Try every matching device until we initialize successfully
                    try
                    {
                        var parser = GetReportParser(identifier.ReportParser) ?? new AuxReportParser();
                        InitializeAuxDevice(dev, identifier, parser);
                        auxIdentifier = identifier;
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Log.Exception(ex);
                        continue;
                    }
                }
            }
            return config.AuxilaryDeviceIdentifiers.Count == 0;
        }

        protected void InitializeDigitizerDevice(HidDevice tabletDevice, DigitizerIdentifier tablet, IReportParser<IDeviceReport> reportParser)
        {
            TabletReader?.Dispose();

            Log.Debug("Detect", $"Using device '{tabletDevice.GetFriendlyName()}'.");
            Log.Debug("Detect", $"Using report parser type '{reportParser.GetType().FullName}'.");
            Log.Debug("Detect", $"Device path: {tabletDevice.DevicePath}");

            foreach (byte index in tablet.InitializationStrings)
            {
                Log.Debug("Device", $"Initializing index {index}");
                tabletDevice.GetDeviceString(index);
            }

            TabletReader = new DeviceReader<IDeviceReport>(tabletDevice, reportParser);
            TabletReader.Report += OnReportRecieved;
            TabletReader.ReadingChanged += (_, state) =>
            {
                Reading?.Invoke(this, state);
                if (state == false)
                    Tablet = null;
            };

            if (tablet.FeatureInitReport is byte[] featureInitReport && featureInitReport.Length > 0)
            {
                Log.Debug("Device", "Setting feature: " + BitConverter.ToString(featureInitReport));
                TabletReader.ReportStream.SetFeature(featureInitReport);
            }

            if (tablet.OutputInitReport is byte[] outputInitReport && outputInitReport.Length > 0)
            {
                Log.Debug("Device", "Setting output: " + BitConverter.ToString(outputInitReport));
                TabletReader.ReportStream.Write(outputInitReport);
            }
        }

        protected void InitializeAuxDevice(HidDevice auxDevice, DeviceIdentifier identifier, IReportParser<IDeviceReport> reportParser)
        {
            AuxReader?.Dispose();

            Log.Debug("Detect", $"Using auxiliary device '{auxDevice.GetFriendlyName()}'.");
            Log.Debug("Detect", $"Using auxiliary report parser type '{reportParser.GetType().Name}'.");
            Log.Debug("Detect", $"Device path: {auxDevice.DevicePath}");

            foreach (byte index in identifier.InitializationStrings)
            {
                Log.Debug("Device", $"Initializing index {index}");
                auxDevice.GetDeviceString(index);
            }

            AuxReader = new DeviceReader<IDeviceReport>(auxDevice, reportParser);
            AuxReader.Report += OnReportRecieved;

            if (identifier.FeatureInitReport is byte[] featureInitReport && featureInitReport.Length > 0)
            {
                try
                {
                    AuxReader.ReportStream.SetFeature(featureInitReport);
                    Log.Debug("Device", $"HID Feature '{BitConverter.ToString(featureInitReport)}' succesfully set");
                }
                catch {}
            }

            if (identifier.OutputInitReport is byte[] outputInitReport && outputInitReport.Length > 0)
            {
                try
                {
                    AuxReader.ReportStream.Write(outputInitReport);
                    Log.Debug("Device", $"HID Output '{BitConverter.ToString(outputInitReport)}' successfully set");
                }
                catch {}
            }
        }

        private IEnumerable<HidDevice> FindMatchingDigitizer(DeviceIdentifier identifier, Dictionary<string, string> attributes)
        {
            return from device in FindMatches(identifier)
                   where DigitizerMatchesAttribute(device, attributes)
                   select device;
        }

        private IEnumerable<HidDevice> FindMatchingAuxiliary(DeviceIdentifier identifier, Dictionary<string, string> attributes)
        {
            return from device in FindMatches(identifier)
                   where AuxMatchesAttribute(device, attributes)
                   select device;
        }

        private IEnumerable<HidDevice> FindMatches(DeviceIdentifier identifier)
        {
            return from device in DeviceList.Local.GetHidDevices()
                where device.CanOpen
                where identifier.VendorID == device.VendorID
                where identifier.ProductID == device.ProductID
                where identifier.InputReportLength == null || identifier.InputReportLength == device.GetMaxInputReportLength()
                where identifier.OutputReportLength == null || identifier.OutputReportLength == device.GetMaxOutputReportLength()
                where DeviceMatchesAllStrings(device, identifier)
                select device;
        }

        private bool DigitizerMatchesAttribute(HidDevice device, Dictionary<string, string> attributes)
        {
            if (SystemInterop.CurrentPlatform != PluginPlatform.Windows)
                return true;

            var devName = device.GetFileSystemName();

            bool interfaceMatches = attributes.ContainsKey("WinInterface") ? Regex.IsMatch(devName, $"&mi_{attributes["WinInterface"]}") : true;
            bool keyMatches = attributes.ContainsKey("WinUsage") ? Regex.IsMatch(devName, $"&col{attributes["WinUsage"]}") : true;

            return interfaceMatches && keyMatches;
        }

        private bool AuxMatchesAttribute(HidDevice device, Dictionary<string, string> attributes)
        {
            return true; // Future proofing
        }

        private bool DeviceMatchesAllStrings(HidDevice device, DeviceIdentifier identifier)
        {
            if (identifier.DeviceStrings == null || identifier.DeviceStrings.Count == 0)
                return true;
            
            foreach (var matchQuery in identifier.DeviceStrings)
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

        protected IReportParser<IDeviceReport> GetReportParser(string parserName) 
        {
            var parserRef = PluginManager.GetPluginReference(parserName);
            return parserRef.Construct<IReportParser<IDeviceReport>>();
        }

        public void Dispose()
        {
            TabletReader.Dispose();
            TabletReader.Report -= OnReportRecieved;
            TabletReader = null;
            
            AuxReader.Dispose();
            AuxReader.Report -= OnReportRecieved;
            AuxReader = null;
        }

        public virtual void OnReportRecieved(object _, IDeviceReport report)
        {
            this.ReportRecieved?.Invoke(this, report);
            if (EnableInput && OutputMode?.Tablet != null)
                if (Interpolators.Count == 0 || (Interpolators.Count > 0 && report is ISyntheticReport) || report is IAuxReport)
                    HandleReport(report);
        }

        public virtual void HandleReport(IDeviceReport report)
        {
            OutputMode.Read(report);
        }
    }
}
