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
            var config = new OpenConfiguration();
            config.SetOption(OpenOption.Priority, OpenPriority.High);
            for (int retries = 3; retries >= 0; retries--)
            {
                if (Tablet.TryOpen(config, out var stream, out var exception))
                {
                    ReportStream = (HidStream)stream;
                    break;
                }
                else
                {                    
                    Log.Fail($"{exception.Message}; {retries} more retries.");
                    Thread.Sleep(1000);
                }
            }

            if (ReportStream == null)
            {
                Log.Fail("Failed to open tablet stream.");
                return;
            }

            WorkerThread = new Thread(Background)
            {
                Name = "TabletDriver Tablet Reader",
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal,
            };
            InputReportLength = tablet.GetMaxInputReportLength();
        }

        public HidDevice Tablet { private set; get; }
        public HidStream ReportStream { private set; get; }

        private Thread WorkerThread;
        public bool Working { protected set; get; }

        private HidDeviceInputReceiver Input;
        public TabletReport RecentReport { private set; get; }
        private int InputReportLength { set; get ; }

        public void Start()
        {
            Working = true;
            WorkerThread.Start();
        }

        public void Stop()
        {
            Working = false;
        }
        
        internal void Abort()
        {
            WorkerThread.Abort();
        }

        private void Background()
        {
            var descriptor = Tablet.GetReportDescriptor();
            Input = descriptor.CreateHidDeviceInputReceiver();
            Input.Start(ReportStream);
            Input.Received += InputReceived;
        }

        private void InputReceived(object sender, EventArgs e)
        {
            var buffer = new byte[InputReportLength];
            if (Input.TryRead(buffer, 0, out var dataReport))
            {
                RecentReport = new TabletReport(buffer);
                // Logging
                if (Driver.Debugging && RecentReport.Lift > 0x80)
                {
                    Log.WriteLine($"<{GetFormattedTime()}> TABLETREPORT", RecentReport.ToString());
                }
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
            Input.Received -= InputReceived;
        }
    }
}