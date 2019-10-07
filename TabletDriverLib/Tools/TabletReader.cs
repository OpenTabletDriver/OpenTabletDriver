using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using HidSharp;
using HidSharp.Reports;
using HidSharp.Reports.Input;

namespace TabletDriverLib.Tools
{
    public class TabletReader
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
        }

        public HidDevice Tablet { private set; get; }
        public HidStream ReportStream { private set; get; }
        
        private Thread WorkerThread;
        public bool Working { protected set; get; }

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
            var buffer = new byte[16];
            Input.TryRead(buffer, 0, out var report);
            if (Driver.Debugging)
                Driver.Log.WriteLine($"<{GetFormattedTime()}> REPORT", BitConverter.ToString(buffer).Replace('-', ' '));
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
    }
}