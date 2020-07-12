using System;
using System.Collections.Generic;
using System.Linq;
using HidSharp;
using NativeLib;
using TabletDriverLib.Interop.Cursor;
using TabletDriverLib.Tablet;
using TabletDriverLib.Vendors;
using TabletDriverPlugin;
using TabletDriverPlugin.Tablet;

namespace TabletDriverLib
{
    public class Driver : IDisposable
    {
        public bool EnableInput { set; get; }
        
        public HidDevice TabletDevice { private set; get; }

        private TabletProperties _tabletProperties;
        public TabletProperties TabletProperties
        {
            private set
            {
                _tabletProperties = value;
                if (OutputMode != null)
                    OutputMode.TabletProperties = TabletProperties;
            }
            get => _tabletProperties;
        }

        public IOutputMode OutputMode { set; get; }
        public DeviceReader<IDeviceReport> TabletReader { private set; get; }
        public DeviceReader<IDeviceReport> AuxReader { private set; get; }

        public bool TryMatch(TabletProperties tablet)
        {
            Log.Write("Detect", $"Searching for tablet '{tablet.Name}'");
            try
            {
                if (TryMatchTablet(tablet, out var tabletDevice, out var identifier, out var tabletParser) || TryMatchAltTablet(tablet, out tabletDevice, out identifier, out tabletParser))
                {
                    InitializeTabletDevice(tablet, tabletDevice, identifier, tabletParser);
                    
                    if (TryMatchAuxDevice(tablet, out var auxDevice, out var auxIdentifier, out var auxParser))
                    {
                        InitializeAuxDevice(auxDevice, auxIdentifier, auxParser);
                    }
                    else if (tablet.AuxilaryDeviceIdentifier is DeviceIdentifier auxDeviceIdentifier)
                    {
                        Log.Write("Detect", "Failed to find auxiliary device, express keys may be unavailable.", true);
                    }
                    
                    return true;
                }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Log.Exception(ex);
                if (PlatformInfo.IsLinux && typeof(UCLogicInfo.VendorIDs).EnumContains(tablet.DigitizerIdentifier.VendorID))
                {
                    Log.Write("Detect", "Failed to get device."
                        + "https://github.com/InfinityGhost/OpenTabletDriver/wiki/Linux-FAQ#notice-for-uclogic-tablet-owners", true);
                }
                else if (PlatformInfo.IsWindows && tablet.DigitizerIdentifier.VendorID == (int)UCLogicInfo.VendorIDs.XP_Pen)
                {
                    Log.Write("Detect", "Failed to get device."
                        + "https://github.com/InfinityGhost/OpenTabletDriver/wiki/Windows-FAQ#my-xp-pen-tablet-fails-to-open-deviceioexception-unable-to-open-hid-class-device");
                }
                else
                {
                    Log.Write("Detect", "Failed to get device. Visit the wiki for more information: "
                        + "https://github.com/InfinityGhost/OpenTabletDriver/wiki", true);
                }
                return false;
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                return false;
            }
            return false;
        }

        internal bool TryMatchTablet(TabletProperties tablet, out HidDevice tabletDevice, out DeviceIdentifier identifier, out IReportParser<IDeviceReport> parser)
        {
            var matches = FindMatches(tablet.DigitizerIdentifier);

            if (matches.Count() > 1)
                Log.Write("Detect", "Warning: More than 1 matching tablet has been found.", true);

            tabletDevice = matches.FirstOrDefault();
            identifier = tablet.DigitizerIdentifier;
            parser = GetReportParser(tablet.DigitizerIdentifier.ReportParser) ?? new TabletReportParser();
            
            return tabletDevice != null;
        }

        internal bool TryMatchAltTablet(TabletProperties tablet, out HidDevice tabletDevice, out DeviceIdentifier identifier, out IReportParser<IDeviceReport> parser)
        {
            if (tablet.AlternateDigitizerIdentifier == null)
            {
                tabletDevice = null;
                identifier = null;
                parser = null;
                return false;
            }

            var matches = FindMatches(tablet.AlternateDigitizerIdentifier);
            
            if (matches.Count() > 1)
                Log.Write("Detect", "Warning: More than 1 matching alternate tablet has been found.", true);

            tabletDevice = matches.FirstOrDefault();
            identifier = tablet.AlternateDigitizerIdentifier;
            parser = GetReportParser(tablet.AlternateDigitizerIdentifier.ReportParser) ?? new TabletReportParser();

            return tabletDevice != null;
        }

        internal bool TryMatchAuxDevice(TabletProperties tablet, out HidDevice auxDevice, out DeviceIdentifier identifier, out IReportParser<IDeviceReport> parser)
        {
            if (tablet.AuxilaryDeviceIdentifier == null)
            {
                auxDevice = null;
                identifier = null;
                parser = null;
                return false;
            }

            var matches = FindMatches(tablet.AuxilaryDeviceIdentifier);

            if (matches.Count() > 1)
                Log.Write("Detect", "Warning: More than 1 matching auxiliary device has been found.", true);
            
            auxDevice = matches.FirstOrDefault();
            identifier = tablet.AuxilaryDeviceIdentifier;
            parser = GetReportParser(tablet.AuxilaryDeviceIdentifier.ReportParser) ?? new AuxReportParser();

            return auxDevice != null;
        }

        internal void InitializeTabletDevice(TabletProperties tablet, HidDevice tabletDevice, DeviceIdentifier identifier, IReportParser<IDeviceReport> reportParser)
        {
            TabletProperties = tablet;
            TabletDevice = tabletDevice;

            Log.Write("Detect", $"Found device '{tabletDevice.GetFriendlyName()}'.");
            Log.Write("Detect", $"Using report parser type '{reportParser.GetType().FullName}'.");
            Log.Debug($"Device path: {TabletDevice.DevicePath}");
            
            TabletReader = new DeviceReader<IDeviceReport>(TabletDevice, reportParser);
            TabletReader.Start();
            TabletReader.Report += HandleReport;
            
            if (identifier.FeatureInitReport is byte[] featureInitReport && featureInitReport.Length > 0)
            {
                Log.Debug("Setting feature: " + BitConverter.ToString(featureInitReport));
                TabletReader.ReportStream.SetFeature(featureInitReport);
            }

            if (identifier.OutputInitReport is byte[] outputInitReport && outputInitReport.Length > 0)
            {
                Log.Debug("Setting output: " + BitConverter.ToString(outputInitReport));
                TabletReader.ReportStream.Write(outputInitReport);
            }
        }

        internal void InitializeAuxDevice(HidDevice auxDevice, DeviceIdentifier identifier, IReportParser<IDeviceReport> reportParser)
        {
            Log.Debug($"Found aux device with report length {auxDevice.GetMaxInputReportLength()}.");
            Log.Debug($"Device path: {auxDevice.DevicePath}");
            
            AuxReader = new DeviceReader<IDeviceReport>(auxDevice, reportParser);
            AuxReader.Start();
            AuxReader.Report += HandleReport;

            if (identifier.FeatureInitReport is byte[] featureInitReport && featureInitReport.Length > 0)
            {
                Log.Debug("Setting aux feature: " + BitConverter.ToString(featureInitReport));
                AuxReader.ReportStream.SetFeature(featureInitReport);
            }

            if (identifier.OutputInitReport is byte[] outputInitReport && outputInitReport.Length > 0)
            {
                Log.Debug("Setting aux output: " + BitConverter.ToString(outputInitReport));
                AuxReader.ReportStream.Write(outputInitReport);
            }
        }

        private IEnumerable<HidDevice> FindMatches(DeviceIdentifier identifier)
        {
            return from device in DeviceList.Local.GetHidDevices()
                where identifier.VendorID == device.VendorID
                where identifier.ProductID == device.ProductID
                where identifier.InputReportLength > 0 ? identifier.InputReportLength == device.GetMaxInputReportLength() : true
                where identifier.OutputReportLength > 0 ? identifier.OutputReportLength == device.GetMaxOutputReportLength() : true
                select device;
        }

        private IReportParser<IDeviceReport> GetReportParser(string parserName) 
        {
            return PluginManager.ConstructObject<IReportParser<IDeviceReport>>(parserName);
        }

        public void Dispose()
        {
            TabletReader.Stop();
            TabletReader.Report -= HandleReport;
            TabletReader = null;
            
            AuxReader.Stop();
            AuxReader.Report -= HandleReport;
            AuxReader = null;
        }

        private void HandleReport(object sender, IDeviceReport report)
        {
            if (EnableInput && OutputMode?.TabletProperties != null)
            {
                OutputMode?.Read(report);
                if (OutputMode is IBindingHandler<IBinding> handler)
                    handler.HandleBinding(report);
            }

            DriverState.PostReport(sender, report);
        }
    }
}
