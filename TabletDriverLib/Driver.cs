using System;
using TabletDriverLib.Tools;
using TabletDriverLib.Tools.Cursor;

namespace TabletDriverLib
{
    public class Driver
    {
        public Driver()
        {
            SetPlatformSpecifics(Environment.OSVersion.Platform);
        }

        public Configuration Configuration { set; get; }
        public bool IsRunning { private set; get; }
        public static bool Debugging { set; get; }
        
        private ICursorHandler CursorHandler;
        public DeviceManager DeviceManager { private set; get; } = new DeviceManager();


        #region Public Methods

        public void Start()
        {
            if (!IsRunning)
            {
                Log.Info("Driver has started.");
                IsRunning = true;
            }
            else
                throw new Exception("The service is already running.");
        }

        public void Stop()
        {
            if (IsRunning)
                IsRunning = false;
        }
        
        #endregion

        private void SetPlatformSpecifics(PlatformID platform)
        {
            switch (platform)
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
                    Log.Fail($"Failed to create a cursor handler for this platform ({platform}).");
                    return;
            }
        }
    }
}
