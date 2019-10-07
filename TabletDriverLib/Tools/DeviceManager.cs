using System;
using System.Collections.Generic;
using System.Linq;
using HidSharp;
using TabletDriverLib.Class;

namespace TabletDriverLib.Tools
{
    public class DeviceManager : IDisposable
    {
        public DeviceManager()
        {
        }

        public HidDevice Tablet { private set; get; }

        public TabletReader TabletReader { private set; get; }

        public IEnumerable<string> GetAllDeviceIdentifiers()
        {
            return Devices.ToList().ConvertAll(
                (device) => $"{device.GetFriendlyName()}: {device.DevicePath}");
        }

        public IEnumerable<HidDevice> Devices
        {
            get => DeviceList.Local.GetHidDevices();
        }

        public bool OpenTablet(string devicePath)
        {
            var device = Devices.FirstOrDefault(d => d.DevicePath == devicePath);
            return Open(device);
        }

        public bool OpenTablet(TabletProperties properties)
        {
            Log.Info("Searching for device...");
            var matching = Devices.Where(d => d.ProductID == properties.ProductID && d.VendorID == properties.VendorID);
            var ordered = matching.OrderBy(d => d.GetFileSystemName());
            var device = ordered.ElementAtOrDefault(properties.DeviceNumber);
            return Open(device);
        }

        internal bool Open(HidDevice device)
        {
            CloseTablet();
            Tablet = device;
            if (Tablet != null)
            {
                Log.Info($"Opened tablet '{Tablet.GetFriendlyName()}'.");
                Log.Info($"Device path: {Tablet.DevicePath}");
                TabletReader = new TabletReader(Tablet);
                return true;
            }
            else
            {
                Log.Fail("Failed to open tablet.");
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
    }
}