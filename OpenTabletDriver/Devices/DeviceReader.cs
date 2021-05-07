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
        public DeviceReader(TabletHandlerID handle, HidDevice device, IReportParser<T> reportParser)
        {
            Handle = handle;
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
        /// The <see cref="TabletHandlerID"/> containing this reader
        /// </summary>
        public TabletHandlerID Handle { protected set; get; }

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
        public event EventHandler<(TabletHandlerID, T)> RawReport;

        /// <summary>
        /// Whether or not the device is actively emitting reports and being parsed.
        /// </summary>
        public bool Reading
        {
            protected set
            {
                if (value != _reading)
                {
                    _reading = value;
                    ReadingChanged?.Invoke(this, Reading);
                }
            }
            get => _reading;
        }

        /// <summary>
        /// Contains the exception that caused the reading change if there is any
        /// </summary>
        public Exception ReadingChangeException;

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
                        RawReport?.Invoke(this, (Handle, debugReport));
                }
            }
            catch (Exception ex)
            {
                ReadingChangeException = ex;
            }
            finally
            {
                Reading = false;
            }
        }

        public void Dispose()
        {
            ReadingChanged = null;
            if (Reading)
                Reading = false;
            ReportStream?.Dispose();
        }
    }
}
