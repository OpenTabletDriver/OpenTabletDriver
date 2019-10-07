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

        public InputManager InputManager { private set; get; } = new InputManager();
        
        public bool IsRunning { private set; get; }
        public static bool Debugging { set; get; }
       
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
                    return;
                case PlatformID.Unix:
                    return;
                case PlatformID.MacOSX:
                    return;
                default:
                    return;
            }
        }
    }
}
