using System;
using System.Collections.Generic;
using System.Linq;
using HidSharp;

namespace TabletDriverLib.Tools
{
    public class DeviceManager : IDisposable
    {
        public DeviceManager()
        {
        }

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
            CloseTablet();
            
            Tablet = Devices.FirstOrDefault(d => d.DevicePath == devicePath);
            if (Tablet != null)
            {
                Driver.Log.WriteLine("DEVICE", $"Device opened: {Tablet.GetFriendlyName()}");
                return true;
            }
            else
            {
                Driver.Log.WriteLine("ERROR", $"Failed to open device through path '{devicePath}'");
                return false;
            }
        }

        public bool CloseTablet()
        {
            if (Tablet != null)
            {
                Tablet = null;
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
        }

        public HidDevice Tablet { private set; get; }
    }
}