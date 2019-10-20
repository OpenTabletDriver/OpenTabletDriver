using System;
using System.Collections.Generic;
using System.Linq;
using HidSharp;
using TabletDriverLib.Class;
using TabletDriverLib.Tools;
using TabletDriverLib.Tools.Cursor;

namespace TabletDriverLib
{
    public class Driver : IDisposable
    {
        public Driver()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.Win32NT:
                case PlatformID.WinCE:
                    CursorHandler = new WindowsCursorHandler();
                    Log.Info("Using Windows cursor handler.");
                    return;
                case PlatformID.Unix:
                    CursorHandler = new XCursorHandler();
                    Log.Info("Using X Window System cursor handler.");
                    return;
                case PlatformID.MacOSX:
                    Log.Info("Using MacOSX cursor handler.");
                    CursorHandler = new MacOSCursorHandler();
                    return;
                default:
                    Log.Fail($"Failed to create a cursor handler for this platform ({Environment.OSVersion.Platform}).");
                    return;
            }
        }

        public static bool Debugging { set; get; }
       
        public HidDevice Tablet { private set; get; }
        public TabletProperties TabletProperties { set; get; }
        private ICursorHandler CursorHandler;

        public event EventHandler<TabletProperties> TabletSuccessfullyOpened;

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

        public bool OpenTablet(TabletProperties tablet)
        {
            Log.Info($"Searching for tablet '{tablet.TabletName}'...");
            var matching = Devices.Where(d => d.ProductID == tablet.ProductID && d.VendorID == tablet.VendorID);
            var ordered = matching.OrderBy(d => d.GetFileSystemName());
            var device = ordered.ElementAtOrDefault(tablet.DeviceNumber);
            TabletProperties = tablet;
            if (TabletArea == null)
            {
                TabletArea = new Area();
                TabletArea.Width = TabletProperties.MaxX;
                TabletArea.Height = TabletProperties.MaxY;
            }
            return Open(device);
        }

        public bool OpenTablet(IEnumerable<TabletProperties> tablets)
        {
            foreach (var tablet in tablets)
            {
                if (OpenTablet(tablet))
                    return true;
            }

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

        #region Areas

        public Area DisplayArea { set; get; } = new Area();
        public Area TabletArea { set; get; } = new Area();

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
            {
                var scaleX = (DisplayArea.Width * TabletProperties.Width) / (TabletArea.Width * TabletProperties.MaxX);
                var scaleY = (DisplayArea.Height * TabletProperties.Height) / (TabletArea.Height * TabletProperties.MaxY);
                var pos = new Point(
                    (scaleX * report.Position.X),
                    (scaleY * report.Position.Y)
                );
                CursorHandler.SetCursorPosition(pos);
            }
        }

        #endregion
    }
}
