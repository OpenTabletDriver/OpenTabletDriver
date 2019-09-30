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
        public bool Debugging { set; get; }
        
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
                if (Debugging)
                {
                    // TODO: Debug functions
                }
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
                    Log.WriteLine("INFO", "Using windows cursor handler.");
                    return;
                case PlatformID.Unix:
                    CursorHandler = new XCursorHandler();
                    Log.WriteLine("INFO", "Using X Window System cursor handler.");
                    return;
                case PlatformID.MacOSX:
                    Log.WriteLine("INFO", "Using MacOSX cursor handler.");
                    CursorHandler = new MacOSCursorHandler();
                    return;
                default:
                    Log.WriteLine("ERROR", $"Failed to create a cursor handler for this platform ({platform}).");
                    return;
            }
        }
    }
}
