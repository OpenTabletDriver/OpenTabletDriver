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

        public IEnumerable<string> GetDeviceIdentifiers()
        {
            return DeviceList.Local.GetAllDevices().ToList().ConvertAll(
                (device) => $"{device.GetFriendlyName()}: {device.DevicePath}");
        }

        public IEnumerable<HidDevice> Devices
        {
            get => DeviceList.Local.GetHidDevices();
        }

        public bool OpenTablet(string devicePath)
        {
            CloseTablet();
            
            ActiveTablet = Devices.First(d => d.DevicePath == devicePath);
            var result = ActiveTablet.TryOpen(out var stream);
            
            ActiveTabletStream = stream;
            Driver.Log.WriteLine("DEVICE", $"Device opened: {ActiveTablet.GetFriendlyName()}");
            return result;
        }
        
        public bool CloseTablet()
        {
            if (ActiveTabletStream != null)
            {
                ActiveTablet = null;
                ActiveTabletStream.Dispose();
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Dispose()
        {
            ActiveTabletStream.Dispose();
        }

        public HidDevice ActiveTablet { private set; get; }
        public HidStream ActiveTabletStream { private set; get; }
    }
}