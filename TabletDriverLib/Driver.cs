using System;
using System.Collections.Generic;
using System.Linq;
using HidSharp;
using TabletDriverLib.Component;
using TabletDriverLib.Interop.Cursor;

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
            if (TabletArea == null || TabletArea.Equals(new Area(0, 0, new Point(0, 0), 0)))
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
        public bool Clipping { set; get; } = true;
        
        public bool BindingsEnabled { set; get; } = false;
        public float TipActivationPressure { set; get; }
        public MouseButton TipButton { set; get; }

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
                // This allows more than one input type in the future
                Absolute(report);
            }
        }

        private void Absolute(TabletReport report)
        {
            var scaleX = (DisplayArea.Width * TabletProperties.Width) / (TabletArea.Width * TabletProperties.MaxX);
            var scaleY = (DisplayArea.Height * TabletProperties.Height) / (TabletArea.Height * TabletProperties.MaxY);
            var reportXOffset = (TabletProperties.MaxX / TabletProperties.Width) * TabletArea.Position.X;
            var reportYOffset = (TabletProperties.MaxY / TabletProperties.Height) * TabletArea.Position.Y;
            var pos = new Point(
                (scaleX * (report.Position.X - reportXOffset)) + DisplayArea.Position.X,
                (scaleY * (report.Position.Y - reportYOffset)) + DisplayArea.Position.Y
            );

            if (Clipping)
            {
                // X position clipping
                if (pos.X > DisplayArea.Width + DisplayArea.Position.X)
                    pos.X = DisplayArea.Width + DisplayArea.Position.X;
                else if (pos.X < DisplayArea.Position.X)
                    pos.X = DisplayArea.Position.X;
                // Y position clipping
                if (pos.Y > DisplayArea.Height + DisplayArea.Position.Y)
                    pos.Y = DisplayArea.Height + DisplayArea.Position.Y;
                else if (pos.Y < DisplayArea.Position.Y)
                    pos.Y = DisplayArea.Position.Y;
            }

            if (BindingsEnabled)
            {
                float pressurePercent = (float)report.Pressure / TabletProperties.MaxPressure * 100f;
                if (pressurePercent >= TipActivationPressure && !CursorHandler.GetMouseButtonState(TipButton))
                {
                    CursorHandler.MouseDown(TipButton);
                }
                else if (pressurePercent < TipActivationPressure && CursorHandler.GetMouseButtonState(TipButton))
                {
                    CursorHandler.MouseUp(TipButton);
                }
            }

            CursorHandler.SetCursorPosition(pos);
        }

        #endregion
    }
}
