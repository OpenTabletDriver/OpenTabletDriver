using System;
using System.Threading;
using HidSharp;
using TabletDriverLib.Tablet;

namespace TabletDriverLib
{
    public class AuxReader : DeviceReader<IDeviceReport>
    {
        public AuxReader(HidDevice auxDevice)
        {
            AuxDevice = auxDevice;
            WorkerThread = new Thread(Main)
            {
                Name = "OpenTabletDriver Aux Device Reader",
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal,
            };
        }

        public HidDevice AuxDevice { private set; get; }
        public override IReportParser<IDeviceReport> Parser { set; get; }

        private Thread WorkerThread;
        
        public override void Start()
        {
            Setup();
            WorkerThread.Start();
        }

        private void Setup()
        {
            var config = new OpenConfiguration();
            config.SetOption(OpenOption.Priority, OpenPriority.Low);
            for (int retries = 3; retries > 0; retries--)
            {
                if (AuxDevice.TryOpen(config, out var stream, out var exception))
                {
                    ReportStream = (HidStream)stream;
                    break;
                }
                else
                {                    
                    Log.Write("Detect", $"{exception}; Retrying {retries} more times", true);
                    Thread.Sleep(1000);
                }
            }
            
            if (ReportStream == null)
            {
                Log.Write("Detect", "Failed to open aux device. Make sure you have required permissions to open device streams.", true);
                return;
            }

            if (Driver.Debugging)
            {
                Log.Debug("InputReportLength: " + AuxDevice.GetMaxInputReportLength());
            }
        }
    }
}