using System;
using System.Collections.Generic;
using System.Linq;
using HidSharp;
using NativeLib;
using TabletDriverLib.Tablet;
using TabletDriverLib.Vendors;

namespace TabletDriverLib
{
    public class Driver : IDisposable
    {
        public static bool Debugging { set; get; }
       
        public HidDevice Tablet { private set; get; }
        public TabletProperties TabletProperties { set; get; }
        public OutputMode OutputMode { set; get; }
        public TabletReader TabletReader { private set; get; }
        public ITabletReportParser TabletReportParser { private set; get; }

        public event EventHandler<TabletProperties> TabletSuccessfullyOpened;

        public IEnumerable<string> GetAllDeviceIdentifiers()
        {
            return Devices.ToList().ConvertAll(
                (device) => $"{device.GetFriendlyName()}: {device.DevicePath}");
        }

        public IEnumerable<HidDevice> Devices => DeviceList.Local.GetHidDevices();

        public bool OpenTablet(string devicePath)
        {
            var device = Devices.FirstOrDefault(d => d.DevicePath == devicePath);
            return OpenTablet(device, new TabletReportParser());
        }

        public bool OpenTablet(TabletProperties tablet)
        {
            Log.Write("Detect", $"Searching for tablet '{tablet.TabletName}'");
            try
            {
                ITabletReportParser parser = new TabletReportParser();
                var matching = Devices.Where(d => d.ProductID == tablet.ProductID && d.VendorID == tablet.VendorID);
                var device = matching.FirstOrDefault(d => d.GetMaxInputReportLength() == tablet.InputReportLength);
                if (device == null && !string.IsNullOrEmpty(tablet.CustomReportParserName))
                {
                    device = matching.FirstOrDefault(d => d.GetMaxInputReportLength() == tablet.CustomInputReportLength);
                    if (device != null)
                        parser = GetTabletReportParser(tablet.CustomReportParserName);
                }
                TabletProperties = tablet;
                return OpenTablet(device, parser);
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

        public bool OpenTablet(IEnumerable<TabletProperties> tablets)
        {
            foreach (var tablet in tablets)
                if (OpenTablet(tablet))
                    return true;

            if (Tablet == null)
                Log.Write("Detect", "No tablets found.", true);
            return false;
        }

        internal bool OpenTablet(HidDevice device, ITabletReportParser reportParser)
        {
            CloseTablet();
            Tablet = device;
            if (Tablet != null)
            {
                Log.Write("Detect", $"Found device '{Tablet.GetFriendlyName()}'.");
                Log.Write("Detect", $"Using report parser type '{reportParser.GetType().FullName}'.");
                if (Debugging)
                {
                    Log.Debug($"Device path: {Tablet.DevicePath}");
                }
                
                TabletReader = new TabletReader(Tablet);
                TabletReader.ReportParser = reportParser;
                TabletReader.Start();
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

        private ITabletReportParser GetTabletReportParser(string fullName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetTypes().FirstOrDefault(t => t.FullName == fullName);
                if (type != null)
                {
                    var ctorResult = type.GetConstructors().FirstOrDefault().Invoke(new object[]{});
                    return ctorResult as ITabletReportParser;
                }
            }
            return null;
        }

        public bool CloseTablet()
        {
            if (Tablet != null)
            {
                Tablet = null;
                TabletReader?.Stop();
                TabletReader?.Dispose();
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Dispose()
        {
            Tablet = null;
            TabletReader?.Abort();
            TabletReader?.Dispose();
        }

        public void BindInput(bool enabled)
        {
            if (enabled)
                TabletReader.Report += Translate;
            else
                TabletReader.Report -= Translate;
        }

        private void Translate(object sender, ITabletReport report)
        {
            if (report.Lift > TabletProperties.MinimumRange)
                OutputMode?.Read(report);
        }
    }
}
