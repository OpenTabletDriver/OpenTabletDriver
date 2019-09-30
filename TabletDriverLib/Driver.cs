using System;
using System.Threading.Tasks;
using TabletDriverLib.Class;

namespace TabletDriverLib
{
    public class Driver
    {
        public Driver()
        {
        }

        public Logger Log { private set; get; } = new Logger();
        public Configuration Configuration { set; get; }

        public bool IsRunning { private set; get; }

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
            Log.WriteLine("Driver has started.");

            while (IsRunning)
            {

            }
        }
    }
}
