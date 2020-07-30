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
        public DeviceReader(HidDevice device, IReportParser<T> reportParser)
        {
            Device = device;
            Parser = reportParser;
            WorkerThread = new Thread(Main)
            {
                Name = "OpenTabletDriver Device Reader",
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal,
            };
        }

        public virtual HidDevice Device { protected set; get; }
        public virtual HidStream ReportStream { protected set; get; }
        public IReportParser<T> Parser { private set; get; }
        public virtual event EventHandler<T> Report;
        
        private bool _reading;
        public bool Reading
        {
            protected set
            {
                _reading = value;
                ReadingChanged?.Invoke(this, Reading);
            }
            get => _reading;
        }
        
        public event EventHandler<bool> ReadingChanged;

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
            bool exclusive = true;
            config.SetOption(OpenOption.Priority, OpenPriority.Low);
            config.SetOption(OpenOption.DeviceExclusive, exclusive);
            if (!Device.TryOpen(config, out var stream, out var exception))
            {
                Log.Write("Detect", "Failed to open device in exclusive mode. Retrying in shared mode.", true);
                exclusive = false;
                config.SetOption(OpenOption.DeviceExclusive, exclusive);
                for (int retries = 2; retries > 0; retries--)
                {
                    if (Device.TryOpen(config, out stream, out exception))
                    {
                        break;
                    }
                    else
                    {
                        Log.Write("Detect", $"{exception}; Retrying {retries} more times", true);
                        Thread.Sleep(1000);
                    }
                }
            }

            ReportStream = (HidStream)stream;

            if (exclusive && ReportStream != null)
                Log.Write("Detect", "Tablet opened in exclusive mode.");
            else if (ReportStream == null)
                Log.Write("Detect", "Failed to open tablet. Make sure you have required permissions to open device streams.", true);
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
            finally
            {
                Reading = false;
            }
        }
    }
}