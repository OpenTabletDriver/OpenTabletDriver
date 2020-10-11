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
                if (TryMatchTablet(config, out var tabletDevice, out var identifier, out var tabletParser))
                {
                    InitializeTabletDevice(tabletDevice, identifier, tabletParser);
                    
                    if (TryMatchAuxDevice(config, out var auxDevice, out var auxIdentifier, out var auxParser))
                    {
                        InitializeAuxDevice(auxDevice, auxIdentifier, auxParser);
                    }
                    else if (config.AuxilaryDeviceIdentifiers?.Count > 0)
                    {
                        Log.Write("Detect", "Failed to find auxiliary device, express keys may be unavailable.", LogLevel.Error);
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

        internal bool TryMatchTablet(TabletConfiguration tablet, out HidDevice tabletDevice, out DigitizerIdentifier identifier, out IReportParser<IDeviceReport> parser)
        {
            tabletDevice = null;
            identifier = null;
            parser = null;
            foreach (var id in tablet.DigitizerIdentifiers)
            {
                var matches = FindMatches(id);

                if (matches.Count() > 1)
                    Log.Write("Detect", "More than 1 matching tablet has been found.", LogLevel.Warning);

                if (matches.FirstOrDefault() is HidDevice dev)
                {
                    tabletDevice = dev;
                    identifier = id;
                    parser = GetReportParser(id.ReportParser) ?? new TabletReportParser();
                    return true;
                }
            }
            return false;
        }

        internal bool TryMatchAuxDevice(TabletConfiguration tablet, out HidDevice auxDevice, out DeviceIdentifier identifier, out IReportParser<IDeviceReport> parser)
        {
            auxDevice = null;
            identifier = null;
            parser = null;
            foreach (var id in tablet.AuxilaryDeviceIdentifiers)
            {
                var matches = FindMatches(id);

                if (matches.Count() > 1)
                    Log.Write("Detect", "More than 1 matching auxiliary device has been found.", LogLevel.Warning);
                
                if (matches.FirstOrDefault() is HidDevice dev)
                {
                    auxDevice = dev;
                    identifier = id;
                    parser = GetReportParser(id.ReportParser) ?? new AuxReportParser();
                }
            }
            return auxDevice != null;
        }

        internal void InitializeTabletDevice(HidDevice tabletDevice, DigitizerIdentifier tablet, IReportParser<IDeviceReport> reportParser)
        {
            TabletReader?.Dispose();
            TabletIdentifier = tablet;

            Log.Write("Detect", $"Found device '{tabletDevice.GetFriendlyName()}'.");
            Log.Write("Detect", $"Using report parser type '{reportParser.GetType().FullName}'.");
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
            
            Log.Debug("Detect", $"Found aux device with report length {auxDevice.GetMaxInputReportLength()}.");
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
