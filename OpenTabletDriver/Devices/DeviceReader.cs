using System;
using System.IO;
using System.Threading;
using HidSharp;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Devices
{
    public class DeviceReader<T> : IDisposable where T : IDeviceReport
    {
        public DeviceReader(HidDevice device, IReportParser<T> reportParser)
        {
            Device = device;
            Parser = reportParser;
            WorkerThread = new Thread(Main)
            {
                Name = "OpenTabletDriver Device Reader",
                IsBackground = true,
                Priority = ThreadPriority.AboveNormal,
            };

            try
            {
                ReportStream = Device.Open();
                WorkerThread.Start();
            }
            catch
            {
                throw new IOException("Failed to open stream");
            }
        }

        public HidDevice Device { protected set; get; }
        public HidStream ReportStream { protected set; get; }
        public IReportParser<T> Parser { private set; get; }
        public event EventHandler<T> Report;

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

        private readonly Thread WorkerThread;

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
                    if (report != null)
                        Report?.Invoke(this, report);
                }
            }
            catch (ObjectDisposedException dex)
            {
                Log.Debug("Device", $"{(string.IsNullOrWhiteSpace(dex.ObjectName) ? "A device stream" : dex.ObjectName)} was disposed.");
            }
            catch (IOException ioex) when (ioex.Message == "I/O disconnected." || ioex.Message == "Operation failed after some time.")
            {
                Log.Write("Device", "Device disconnected.");
            }
            catch (ArgumentOutOfRangeException)
            {
                Log.Write("Device", "Not enough report data returned by the device. Was it disconnected?");
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

        public void Dispose()
        {
            Reading = false;
            ReportStream?.Dispose();
        }
    }
}
