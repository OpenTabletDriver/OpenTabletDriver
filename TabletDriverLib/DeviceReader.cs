using System;
using System.Threading;
using HidSharp;
using TabletDriverLib.Contracts;
using TabletDriverPlugin;
using TabletDriverPlugin.Tablet;


namespace TabletDriverLib
{
    public class DeviceReader<T> : IDeviceReader<T>, IDisposable where T : IDeviceReport
    {
        public DeviceReader(HidDevice device)
        {
            Device = device;
            WorkerThread = new Thread(Main)
            {
                Name = "OpenTabletDriver Device Reader",
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal,
            };
        }


        public virtual HidDevice Device { protected set; get; }
        public virtual HidStream ReportStream { protected set; get; }

        public bool Reading { protected set; get; }
        public IReportParser<T> Parser { set; get; }
        public virtual event EventHandler<T> Report;

        private Thread WorkerThread;

        public virtual void Start()
        {
            Setup();
            WorkerThread.Start();
        }

        public virtual void Stop()
        {
            Reading = false;
        }

        public virtual void Dispose()
        {
            Stop();
            ReportStream?.Dispose();
        }

        private void Setup()
        {
            var config = new OpenConfiguration();
            config.SetOption(OpenOption.Priority, OpenPriority.Low);
            for (int retries = 3; retries > 0; retries--)
            {
                if (Device.TryOpen(config, out var stream, out var exception))
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
                Log.Write("Detect", "Failed to open tablet. Make sure you have required permissions to open device streams.", true);
                return;
            }

            if (Driver.Debugging)
            {
                Log.Debug("InputReportLength: " + Device.GetMaxInputReportLength());
            }
        }

        protected void Main()
        {
            try
            {
                Reading = true;
                ReportStream.ReadTimeout = int.MaxValue;
                while (Reading)
                {
                    var data = ReportStream.Read();
                    var report = Parser.Parse(data);
                    Report?.Invoke(this, report);
                }
            }
            catch (ObjectDisposedException dex)
            {
                Log.Debug($"{(string.IsNullOrWhiteSpace(dex.ObjectName) ? "A device stream" : dex.ObjectName)} was disposed.");
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }
    }
}