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
            workerThread = new Thread(Main)
            {
                Name = "OpenTabletDriver Device Reader",
                IsBackground = true,
                Priority = ThreadPriority.AboveNormal,
            };

            try
            {
                ReportStream = Device.Open();
                workerThread.Start();
            }
            catch
            {
                throw new IOException("Failed to open stream");
            }
        }

        private readonly Thread workerThread;
        private bool _reading;

        /// <summary>
        /// The HID endpoint in which is reporting data in the <see cref="ReportStream"/>.
        /// </summary>
        public HidDevice Device { protected set; get; }

        /// <summary>
        /// The raw HID report stream.
        /// </summary>
        public HidStream ReportStream { protected set; get; }

        /// <summary>
        /// The <see cref="IReportParser{T}"/> in which the device reports will be parsed with.
        /// </summary>
        public IReportParser<T> Parser { private set; get; }
        
        /// <summary>
        /// Whether or not to make an extra cloned report with data left unmodified.
        /// </summary>
        public bool RawClone { set; get; }

        /// <summary>
        /// Invoked when a new report comes in from the device.
        /// </summary>
        public event EventHandler<T> Report;

        /// <summary>
        /// Invoked when a new report comes in from the device.
        /// </summary>
        /// <remarks>
        /// This will only be invoked when <see cref="RawClone"/> is set to true.
        /// This report is not meant in any way to be modified, as it is supposed to represent the original data.
        /// </remarks>
        public event EventHandler<T> RawReport;

        /// <summary>
        /// Whether or not the device is actively emitting reports and being parsed.
        /// </summary>
        public bool Reading
        {
            protected set
            {
                _reading = value;
                ReadingChanged?.Invoke(this, Reading);
            }
            get => _reading;
        }

        /// <summary>
        /// Invoked when <see cref="Reading"/> is changed.
        /// </summary>
        public event EventHandler<bool> ReadingChanged;

        protected void Main()
        {
            try
            {
                Reading = true;
                ReportStream.ReadTimeout = int.MaxValue;
                while (Reading)
                {
                    var data = ReportStream.Read();
                    if (Parser.Parse(data) is T report)
                        Report?.Invoke(this, report);
                    
                    // We create a clone of the report to avoid data being modified on the tablet debugger.
                    if (RawClone && RawReport != null && Parser.Parse(data) is T debugReport)
                        RawReport?.Invoke(this, debugReport);
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
