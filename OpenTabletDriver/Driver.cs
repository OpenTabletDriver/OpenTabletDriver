using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HidSharp;
using OpenTabletDriver.Native;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Reflection;
using OpenTabletDriver.Tablet;
using OpenTabletDriver.Vendors;

namespace OpenTabletDriver
{
    public class Driver : IDriver, IDisposable
    {
        public Driver()
        {
            Info.GetDriverInstance = () => this;
        }
        
        public event EventHandler<bool> Reading;
        public event EventHandler<IDeviceReport> ReportRecieved;
        
        public bool EnableInput { set; get; }

        public TabletConfiguration Tablet { private set; get; }

        private DigitizerIdentifier tabletIdentifier;
        public DigitizerIdentifier TabletIdentifier
        {
            private set
            {
                this.tabletIdentifier = value;
                if (OutputMode != null)
                    OutputMode.Digitizer = value;
            }
            get => this.tabletIdentifier;
        }

        public DeviceIdentifier AuxiliaryIdentifier { private set; get; }

        public IOutputMode OutputMode { set; get; }
        
        public DeviceReader<IDeviceReport> TabletReader { private set; get; }
        public DeviceReader<IDeviceReport> AuxReader { private set; get; }

        public bool TryMatch(TabletConfiguration config)
        {
            Log.Write("Detect", $"Searching for tablet '{config.Name}'");
            try
            {
                if (TryMatchTablet(config))
                {
                    Log.Write("Detect", $"Found tablet '{config.Name}'");
                    if (!TryMatchAuxDevice(config))
                    {
                        Log.Write("Detect", "Failed to find auxiliary device, express keys may be unavailable.", LogLevel.Warning);
                    }
                    
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
            return false;
        }

        internal bool TryMatchTablet(TabletConfiguration config)
        {
            foreach (var identifier in config.DigitizerIdentifiers)
            {
                var matches = FindMatches(identifier);

                if (matches.Count() > 1)
                    Log.Write("Detect", "More than 1 matching tablet has been found.", LogLevel.Warning);

                foreach (HidDevice dev in matches)
                {
                    // Try every matching device until we initialize successfully
                    try
                    {
                        var parser = GetReportParser(identifier.ReportParser) ?? new TabletReportParser();
                        InitializeDigitizerDevice(dev, identifier, parser);
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

        internal bool TryMatchAuxDevice(TabletConfiguration config)
        {
            foreach (var identifier in config.AuxilaryDeviceIdentifiers)
            {
                var matches = FindMatches(identifier);

                if (matches.Count() > 1)
                    Log.Write("Detect", "More than 1 matching auxiliary device has been found.", LogLevel.Warning);

                foreach (HidDevice dev in matches)
                {
                    // Try every matching device until we initialize successfully
                    try
                    {
                        var parser = GetReportParser(identifier.ReportParser) ?? new AuxReportParser();
                        InitializeAuxDevice(dev, identifier, parser);
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

        internal void InitializeDigitizerDevice(HidDevice tabletDevice, DigitizerIdentifier tablet, IReportParser<IDeviceReport> reportParser)
        {
            TabletReader?.Dispose();
            TabletIdentifier = tablet;

            Log.Debug("Detect", $"Using device '{tabletDevice.GetFriendlyName()}'.");
            Log.Debug("Detect", $"Using report parser type '{reportParser.GetType().FullName}'.");
            Log.Debug("Detect", $"Device path: {tabletDevice.DevicePath}");

            if (tablet.InitializationStrings.Count > 0)
            {
                foreach (var index in tablet.InitializationStrings)
                {
                    Log.Debug("Detect", $"Initializing index {index}");
                    tabletDevice.GetDeviceString(index);
                }
            }
            
            TabletReader = new DeviceReader<IDeviceReport>(tabletDevice, reportParser);
            TabletReader.Start();
            TabletReader.Report += OnReport;
            TabletReader.ReadingChanged += (sender, e) => Reading?.Invoke(sender, e);
            
            if (tablet.FeatureInitReport is byte[] featureInitReport && featureInitReport.Length > 0)
            {
                Log.Debug("Detect", "Setting feature: " + BitConverter.ToString(featureInitReport));
                TabletReader.ReportStream.SetFeature(featureInitReport);
            }

            if (tablet.OutputInitReport is byte[] outputInitReport && outputInitReport.Length > 0)
            {
                Log.Debug("Detect", "Setting output: " + BitConverter.ToString(outputInitReport));
                TabletReader.ReportStream.Write(outputInitReport);
            }
        }

        internal void InitializeAuxDevice(HidDevice auxDevice, DeviceIdentifier identifier, IReportParser<IDeviceReport> reportParser)
        {
            AuxReader?.Dispose();
            AuxiliaryIdentifier = identifier;
            
            Log.Debug("Detect", $"Using auxiliary device '{auxDevice.GetFriendlyName()}'.");
            Log.Debug("Detect", $"Using auxiliary report parser type '{reportParser.GetType().Name}'.");
            Log.Debug("Detect", $"Device path: {auxDevice.DevicePath}");

            if (identifier.InitializationStrings.Count > 0)
            {
                foreach (var index in identifier.InitializationStrings)
                {
                    Log.Debug("Detect", $"Initializing index {index}");
                    auxDevice.GetDeviceString(index);
                }
            }
            
            AuxReader = new DeviceReader<IDeviceReport>(auxDevice, reportParser);
            AuxReader.Start();
            AuxReader.Report += OnReport;

            if (identifier.FeatureInitReport is byte[] featureInitReport && featureInitReport.Length > 0)
            {
                Log.Debug("Detect", "Setting aux feature: " + BitConverter.ToString(featureInitReport));
                AuxReader.ReportStream.SetFeature(featureInitReport);
            }

            if (identifier.OutputInitReport is byte[] outputInitReport && outputInitReport.Length > 0)
            {
                Log.Debug("Detect", "Setting aux output: " + BitConverter.ToString(outputInitReport));
                AuxReader.ReportStream.Write(outputInitReport);
            }
        }

        private IEnumerable<HidDevice> FindMatches(DeviceIdentifier identifier)
        {
            return from device in DeviceList.Local.GetHidDevices()
                where identifier.VendorID == device.VendorID
                where identifier.ProductID == device.ProductID
                where identifier.InputReportLength == null ? true : identifier.InputReportLength == device.GetMaxInputReportLength()
                where identifier.OutputReportLength == null ? true : identifier.OutputReportLength == device.GetMaxOutputReportLength()
                where DeviceMatchesAllStrings(device, identifier)
                select device;
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

        private IReportParser<IDeviceReport> GetReportParser(string parserName) 
        {
            var parserRef = new PluginReference(parserName);
            return parserRef.Construct<IReportParser<IDeviceReport>>();
        }

        public void Dispose()
        {
            TabletReader.Stop();
            TabletReader.Report -= OnReport;
            TabletReader = null;
            
            AuxReader.Stop();
            AuxReader.Report -= OnReport;
            AuxReader = null;
        }

        private void OnReport(object sender, IDeviceReport report)
        {
            if (EnableInput && OutputMode?.Digitizer != null)
            {
                OutputMode?.Read(report);
                if (OutputMode is IBindingHandler<IBinding> handler)
                    handler.HandleBinding(report);
            }

            this.ReportRecieved?.Invoke(this, report);
        }
    }
}
