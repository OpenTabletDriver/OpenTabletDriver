using System;
using System.Collections.Generic;
using System.Linq;
using HidSharp;
using TabletDriverLib.Class;
using TabletDriverLib.Tools.Cursor;

namespace TabletDriverLib.Tools
{
    public class InputManager : IDisposable
    {
        public InputManager()
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

        public HidDevice Tablet { private set; get; }
        public TabletProperties TabletProperties { set; get; }
        private ICursorHandler CursorHandler;

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
            TabletProperties = properties;
            if (TabletArea == null)
            {
                TabletArea = new Area();
                TabletArea.Width = TabletProperties.MaxX;
                TabletArea.Height = TabletProperties.MaxY;
            }
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
                TabletReader.Start();
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

        #region Areas

        public Area DisplayArea { set; get; }
        public Area TabletArea { set; get; }

        public void BindPositions(bool val)
        {
            if (val)
            {
                TabletReader.Report += Translate;
            }
            else
            {
                TabletReader.Report -= Translate;
            }
        }

        private void Translate(object sender, TabletReport report)
        {
            if (report.Lift > TabletProperties.MinimumRange)
            {
                var scaleX = DisplayArea.Width / TabletArea.Width;
                var scaleY = DisplayArea.Height / TabletArea.Height;
                var pos = new Point(
                    (scaleX * report.Position.X),
                    (scaleY * report.Position.Y));
                if (Driver.Debugging)
                {
                    Log.WriteLine("POSITION", pos);
                }
                CursorHandler.SetCursorPosition(pos);
            }
        }

        #endregion
    }
}