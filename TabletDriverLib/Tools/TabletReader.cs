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
            ReportStream = Tablet.Open();
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

        private int InputReportLength;
        private byte[] LastReport;
        private HidDeviceInputReceiver Input;

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
                // Set last report to the read values
                LastReport = buffer;
                var report = new TabletReport(buffer);
                // Logging
                if (Driver.Debugging)
                {
                    // Driver.Log.WriteLine($"<{GetFormattedTime()}> REPORT", BitConverter.ToString(buffer).Replace('-', ' '));
                    if (report.InRange)
                    {
                        Driver.Log.WriteLine(
                            $"<{GetFormattedTime()}> TABLETREPORT",
                            $"InRange:{report.InRange}, Position:[{report.Position}], Pressure:{report.Pressure}");
                    }
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