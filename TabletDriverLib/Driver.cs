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
        public static bool Debugging { set; get; }
        public static bool RawReports { set; get; }
        public bool BindingEnabled { set; get; }
        
        public HidDevice Tablet { private set; get; }
        public TabletProperties TabletProperties { set; get; }

        public IOutputMode OutputMode { set; get; }
        public DeviceReader<IDeviceReport> TabletReader { private set; get; }
        public DeviceReader<IDeviceReport> AuxReader { private set; get; }

        public event EventHandler<TabletProperties> TabletSuccessfullyOpened;

        public IEnumerable<HidDevice> Devices => DeviceList.Local.GetHidDevices();

        public bool Open(TabletProperties tablet)
        {
            Log.Write("Detect", $"Searching for tablet '{tablet.TabletName}'");
            try
            {
                var matching = Devices.Where(d => d.ProductID == tablet.ProductID && d.VendorID == tablet.VendorID);
                var tabletDevice = matching.FirstOrDefault(d => d.GetMaxInputReportLength() == tablet.InputReportLength);
                var parser = PluginManager.ConstructObject<IDeviceReportParser>(tablet.ReportParserName) ?? new TabletReportParser();
                if (tabletDevice == null && !string.IsNullOrEmpty(tablet.CustomReportParserName))
                {
                    tabletDevice = matching.FirstOrDefault(d => d.GetMaxInputReportLength() == tablet.CustomInputReportLength);
                    if (tabletDevice != null)
                        parser = PluginManager.ConstructObject<IDeviceReportParser>(tablet.CustomReportParserName);
                }
                TabletProperties = tablet;

                var tabletOpened = Open(tabletDevice, parser);
                if (tabletOpened && tablet.AuxReportLength > 0)
                {
                    var auxDevice = matching.FirstOrDefault(d => d.GetMaxInputReportLength() == tablet.AuxReportLength);
                    var auxReportParser = PluginManager.ConstructObject<IDeviceReportParser>(tablet.AuxReportParserName) ?? new AuxReportParser();
                    OpenAux(auxDevice, auxReportParser);
                }
                return tabletOpened;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                if (Debugging)
                    Log.Exception(ex);
                if (PlatformInfo.IsLinux && typeof(UCLogicInfo.VendorIDs).EnumContains(tablet.VendorID))
                {
                    Log.Write("Detect", "Failed to get device input report length. "
                        + "Ensure the 'hid-uclogic' module is disabled.", true);
                }
                else
                {
                    Log.Write("Detect", "Failed to get device input report length. "
                        + "Visit the wiki (https://github.com/InfinityGhost/OpenTabletDriver/wiki) for more information.", true);
                }
                return false;
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                return false;
            }
        }

        public bool Open(IEnumerable<TabletProperties> tablets)
        {
            foreach (var tablet in tablets)
                if (Open(tablet))
                    return true;

            if (Tablet == null)
                Log.Write("Detect", "No tablets found.", true);
            return false;
        }

        internal bool Open(HidDevice device, IDeviceReportParser reportParser)
        {
            Close();
            Tablet = device;
            if (Tablet != null)
            {
                Log.Write("Detect", $"Found device '{Tablet.GetFriendlyName()}'.");
                Log.Write("Detect", $"Using report parser type '{reportParser.GetType().FullName}'.");
                if (Debugging)
                {
                    Log.Debug($"Device path: {Tablet.DevicePath}");
                }
                
                TabletReader = new DeviceReader<IDeviceReport>(Tablet);
                TabletReader.Parser = reportParser;
                TabletReader.Start();
                TabletReader.Report += HandleReport;
                
                if (TabletProperties.FeatureInitReport != null && TabletProperties.FeatureInitReport.Length > 0)
                {
                    Log.Write("Debug", $"Setting feature: " + BitConverter.ToString(TabletProperties.FeatureInitReport));
                    TabletReader.ReportStream.SetFeature(TabletProperties.FeatureInitReport);
                }

                // Post tablet opened event
                TabletSuccessfullyOpened?.Invoke(this, TabletProperties);
                return true;
            }
            else
            {
                if (Debugging)
                    Log.Write("Detect", "Tablet not found.", true);
                return false;
            }
        }

        internal bool OpenAux(HidDevice auxDevice, IDeviceReportParser reportParser)
        {
            if (auxDevice != null)
            {
                if (Debugging)
                {
                    Log.Debug($"Found aux device with report length {auxDevice.GetMaxInputReportLength()}.");
                    Log.Debug($"Device path: {auxDevice.DevicePath}");
                }
                
                AuxReader = new DeviceReader<IDeviceReport>(auxDevice);
                AuxReader.Parser = reportParser;
                AuxReader.Start();
                AuxReader.Report += HandleReport;
                return true;
            }
            else
            {
                if (Debugging)
                    Log.Write("Detect", "Failed to open aux device.");
                return false;
            }
        }

        public bool Close()
        {
            Tablet = null;
            TabletReader?.Dispose();
            AuxReader?.Dispose();
            return true;
        }

        public void Dispose()
        {
            Close();
            TabletReader.Report -= HandleReport;
            AuxReader.Report -= HandleReport;
        }

        private void HandleReport(object sender, IDeviceReport report)
        {
            if (BindingEnabled && OutputMode?.TabletProperties != null)
            {
                OutputMode?.Read(report);
                if (OutputMode is IBindingHandler<IBinding> binding)
                    binding.HandleBinding(report);
            }
        }
    }
}
