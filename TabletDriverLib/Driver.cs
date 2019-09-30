using System;
using System.Threading.Tasks;
using TabletDriverLib.Class;
using TabletDriverLib.Tools.Cursor;

namespace TabletDriverLib
{
    public class Driver
    {
        public Driver()
        {
            SetPlatformSpecifics(Environment.OSVersion.Platform);
        }

        internal Logger Log { set; get; } = new Logger();
        public Configuration Configuration { set; get; }
        public bool IsRunning { private set; get; }
        
        private ICursorHandler CursorHandler;


        #region Public Methods

        public async void Start()
        {
            if (!IsRunning)
            {
                IsRunning = true;
                await Task.Run(Main);
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

        private void Main()
        {
            Log.WriteLine("INFO", "Driver has started.");

            while (IsRunning)
            {
                // TODO: Main() function
            }
        }

        private void SetPlatformSpecifics(PlatformID platform)
        {
            switch (platform)
            {
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.Win32NT:
                case PlatformID.WinCE:
                    CursorHandler = new WindowsCursorHandler();
                    return;
                case PlatformID.Unix:
                    CursorHandler = new XCursorHandler();
                    return;
                case PlatformID.MacOSX:
                    CursorHandler = new MacOSCursorHandler();
                    return;
                default:
                    Log.WriteLine("ERROR", $"Failed to create a cursor handler for this platform ({platform}).");
                    return;
            }
        }
    }
}
