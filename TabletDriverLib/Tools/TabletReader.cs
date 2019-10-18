using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using HidSharp;
using HidSharp.Reports;
using HidSharp.Reports.Input;
using TabletDriverLib.Class;

namespace TabletDriverLib.Tools
{
    public class TabletReader : IDisposable
    {
        public TabletReader(HidDevice tablet)
        {
            Tablet = tablet;
            WorkerThread = new Thread(Background)
            {
                Name = "TabletDriver Tablet Reader",
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal,
            };
        }

        public HidDevice Tablet { private set; get; }
        public HidStream ReportStream { private set; get; }

        public event EventHandler<TabletReport> Report;

        private Thread WorkerThread;
        public bool Working { protected set; get; }

        private HidDeviceInputReceiver Input;
        
        private int InputReportLength { set; get; }

        private bool _handled;
        public bool ReadingInput
        {
            set
            {
                if (value && !_handled)
                {
                    Input.Received += OnInputReceived;
                    _handled = true;
                }
                else if (!value && _handled)
                {
                    Input.Received -= OnInputReceived;
                    _handled = false;
                }
            }
            get => _handled;
        }

        public void Start()
        {
            WorkerThread.Start();
        }

        public void Stop()
        {
            ReadingInput = false;
        }
        
        internal void Abort()
        {
            WorkerThread.Abort();
        }

        private void Background()
        {
            var config = new OpenConfiguration();
            config.SetOption(OpenOption.Priority, OpenPriority.Low);
            for (int retries = 3; retries > 0; retries--)
            {
                if (Tablet.TryOpen(config, out var stream, out var exception))
                {
                    ReportStream = (HidStream)stream;
                    break;
                }
                else
                {                    
                    Log.Fail($"{exception.Message} {retries} more retries.");
                    Thread.Sleep(1000);
                }
            }
            
            if (ReportStream == null)
            {
                Log.Fail("Failed to open tablet. Make sure you have required permissions to open device streams.");
                return;
            }

            InputReportLength = Tablet.GetMaxInputReportLength();

            var descriptor = Tablet.GetReportDescriptor();
            Input = descriptor.CreateHidDeviceInputReceiver();
            Input.Start(ReportStream);
            ReadingInput = true;
        }

        private void OnInputReceived(object sender, EventArgs e)
        {
            var buffer = new byte[InputReportLength];
            if (Input.TryRead(buffer, 0, out var dataReport))
            {
                var report = new TabletReport(buffer);
                Report?.Invoke(this, report);
                // Logging
                if (Driver.Debugging)
                    Log.WriteLine($"<{GetFormattedTime()}> TABLETREPORT", report.ToString());
            }
        }

        private static string GetFormattedTime()
        {
            var hr = string.Format(GetNumberFormat(2), DateTime.Now.Hour);
            var min = string.Format(GetNumberFormat(2), DateTime.Now.Minute);
            var sec = string.Format(GetNumberFormat(2), DateTime.Now.Second);
            var ms = string.Format(GetNumberFormat(3), DateTime.Now.Millisecond);
            return $"{hr}:{min}:{sec}.{ms}";
        }

        private static string GetNumberFormat(int digits)
        {
            var zeros = Enumerable.Repeat('0', digits).ToArray();
            return "{0:" + new string(zeros) + "}";
        }

        public void Dispose()
        {
            ReadingInput = false;
        }
    }
}