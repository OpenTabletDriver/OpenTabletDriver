using System;
using System.ComponentModel;
using System.IO;
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
            var buffer = new byte[128];
            Input.TryRead(buffer, 0, out var report);
            ReadReport(ref report);
        }

        private void ReadReport(ref Report report)
        {
            var parser = new DeviceItemInputParser(report.DeviceItem);
            var buffer = new byte[128];
            if (parser.TryParseReport(buffer, 0, report))
            {
                Driver.Log.WriteLine("READ", $"ValueCount:{parser.ValueCount}");
            }
            else
            {
                Driver.Log.WriteLine("FAIL", "Failed to read report.");
            }
        }
    }
}