using System;
using System.Collections.Generic;
using System.Linq;
using HidSharp;
using TabletDriverLib.Component;
using TabletDriverLib.Interop.Input;

namespace TabletDriverLib
{
    public class Driver : IDisposable
    {
        public Driver()
        {
        }

        public static bool Debugging { set; get; }
       
        public HidDevice Tablet { private set; get; }
        public TabletProperties TabletProperties { set; get; }
        private IInputHandler InputHandler;
        public OutputMode OutputMode { set; get; }
        public TabletReader TabletReader { private set; get; }

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
            return Open(device);
        }

        public bool OpenTablet(TabletProperties tablet)
        {
            Log.Info($"Searching for tablet '{tablet.TabletName}'...");
            var matching = Devices.Where(d => d.ProductID == tablet.ProductID && d.VendorID == tablet.VendorID);
            var device = matching.FirstOrDefault(d => d.GetMaxInputReportLength() == tablet.InputReportLength);
            TabletProperties = tablet;
            return Open(device);
        }

        public bool OpenTablet(IEnumerable<TabletProperties> tablets)
        {
            foreach (var tablet in tablets)
                if (OpenTablet(tablet))
                    return true;

            if (Tablet == null)
                Log.Fail("No configured tablets found.");
            return false;
        }

        internal bool Open(HidDevice device)
        {
            CloseTablet();
            Tablet = device;
            if (Tablet != null)
            {
                Log.Write($" Found: {Tablet.GetFriendlyName()}.");
                if (Debugging)
                {
                    Log.Info($"Device path: {Tablet.DevicePath}");
                }
                
                TabletReader = new TabletReader(Tablet);
                TabletReader.Start();
                // Post tablet opened event
                TabletSuccessfullyOpened?.Invoke(this, TabletProperties);
                return true;
            }
            else
            {
                Log.Write(" Not found");
                return false;
            }
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

        private void Translate(object sender, TabletReport report)
        {
            if (report.Lift > TabletProperties.MinimumRange)
                OutputMode.Read(report);
        }
    }
}
