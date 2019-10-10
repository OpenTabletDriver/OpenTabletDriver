using System;
using TabletDriverLib.Tools;
using TabletDriverLib.Tools.Cursor;

namespace TabletDriverLib
{
    public class Driver
    {
        public Driver()
        {
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
    }
}
