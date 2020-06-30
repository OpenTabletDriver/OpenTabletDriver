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
            Log.Write("Detect", $"Searching for tablet '{tablet.TabletName}'");
            try
            {
                if (TryMatchTablet(tablet, out var tabletDevice, out var tabletParser) || TryMatchAltTablet(tablet, out tabletDevice, out tabletParser))
                {
                    InitializeTabletDevice(tablet, tabletDevice, tabletParser);
                    
                    if (TryMatchAuxDevice(tablet, out var auxDevice, out var auxParser))
                    {
                        InitializeAuxDevice(auxDevice, auxParser);
                    }
                    else if (tablet.AuxReportLength > 0)
                    {
                        Log.Write("Detect", "Failed to find auxiliary device, express keys may be unavailable.", true);
                    }
                    
                    return true;
                }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Log.Exception(ex);
                if (PlatformInfo.IsLinux && typeof(UCLogicInfo.VendorIDs).EnumContains(tablet.VendorID))
                {
                    Log.Write("Detect", "Failed to get device."
                        + "https://github.com/InfinityGhost/OpenTabletDriver/wiki/Linux-FAQ#notice-for-uclogic-tablet-owners", true);
                }
                else if (PlatformInfo.IsWindows && tablet.VendorID == (int)UCLogicInfo.VendorIDs.XP_Pen)
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

        internal bool TryMatchTablet(TabletProperties tablet, out HidDevice tabletDevice, out IReportParser<IDeviceReport> parser)
        {
            var matches = from device in DeviceList.Local.GetHidDevices()
                where tablet.VendorID == device.VendorID
                where tablet.ProductID == device.ProductID
                where tablet.InputReportLength == device.GetMaxInputReportLength()
                where tablet.OutputReportLength > 0 ? device.GetMaxOutputReportLength() == tablet.OutputReportLength : true
                select device;

            if (matches.Count() > 1)
                Log.Write("Detect", "Warning: More than 1 matching tablet has been found.", true);

            tabletDevice = matches.FirstOrDefault();
            parser = PluginManager.ConstructObject<IReportParser<IDeviceReport>>(tablet.ReportParserName) ?? new TabletReportParser();
            
            return tabletDevice != null;
        }

        internal bool TryMatchAltTablet(TabletProperties tablet, out HidDevice tabletDevice, out IReportParser<IDeviceReport> parser)
        {
            if (string.IsNullOrWhiteSpace(tablet.CustomReportParserName))
            {
                tabletDevice = null;
                parser = null;
                return false;
            }

            var matches = from device in DeviceList.Local.GetHidDevices()
                where tablet.VendorID == device.VendorID
                where tablet.ProductID == device.ProductID
                where tablet.CustomInputReportLength == device.GetMaxInputReportLength()
                select device;
            
            if (matches.Count() > 1)
                Log.Write("Detect", "Warning: More than 1 matching alternate tablet has been found.", true);

            tabletDevice = matches.FirstOrDefault();
            parser = PluginManager.ConstructObject<IReportParser<IDeviceReport>>(tablet.CustomReportParserName);

            return tabletDevice != null;
        }

        internal bool TryMatchAuxDevice(TabletProperties tablet, out HidDevice auxDevice, out IReportParser<IDeviceReport> parser)
        {
            if (tablet.AuxReportLength == 0)
            {
                auxDevice = null;
                parser = null;
                return false;
            }

            var matches = from device in DeviceList.Local.GetHidDevices()
                where tablet.VendorID == device.VendorID
                where tablet.ProductID == device.ProductID
                where tablet.AuxReportLength == device.GetMaxInputReportLength()
                select device;

            if (matches.Count() > 1)
                Log.Write("Detect", "Warning: More than 1 matching auxiliary device has been found.", true);
            
            auxDevice = matches.FirstOrDefault();
            parser = PluginManager.ConstructObject<IReportParser<IDeviceReport>>(tablet.AuxReportParserName) ?? new AuxReportParser();

            return auxDevice != null;
        }

        internal void InitializeTabletDevice(TabletProperties tablet, HidDevice tabletDevice, IReportParser<IDeviceReport> reportParser)
        {
            TabletProperties = tablet;
            TabletDevice = tabletDevice;

            Log.Write("Detect", $"Found device '{tabletDevice.GetFriendlyName()}'.");
            Log.Write("Detect", $"Using report parser type '{reportParser.GetType().FullName}'.");
            Log.Debug($"Device path: {TabletDevice.DevicePath}");
            
            TabletReader = new DeviceReader<IDeviceReport>(TabletDevice, reportParser);
            TabletReader.Start();
            TabletReader.Report += HandleReport;
            
            if (TabletProperties.FeatureInitReport != null && TabletProperties.FeatureInitReport.Length > 0)
            {
                Log.Debug($"Setting feature: " + BitConverter.ToString(TabletProperties.FeatureInitReport));
                TabletReader.ReportStream.SetFeature(TabletProperties.FeatureInitReport);
            }
        }

        internal void InitializeAuxDevice(HidDevice auxDevice, IReportParser<IDeviceReport> reportParser)
        {
            Log.Debug($"Found aux device with report length {auxDevice.GetMaxInputReportLength()}.");
            Log.Debug($"Device path: {auxDevice.DevicePath}");
            
            AuxReader = new DeviceReader<IDeviceReport>(auxDevice, reportParser);
            AuxReader.Start();
            AuxReader.Report += HandleReport;
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
