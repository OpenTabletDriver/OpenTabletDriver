using System;
using System.IO;
using System.Threading;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Devices;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Devices
{
    public class DeviceReader<T> : IDisposable where T : IDeviceReport
    {
        public DeviceReader(IDeviceEndpoint endpoint, IReportParser<T> reportParser)
        {
            Endpoint = endpoint;
            Parser = reportParser;
            workerThread = new Thread(Main)
            {
                Name = "OpenTabletDriver Device Reader",
                IsBackground = true,
                Priority = ThreadPriority.AboveNormal
            };
        }

        private readonly Thread workerThread;
        private bool initialized, connected;

        /// <summary>
        /// The device endpoint in which is reporting data in the <see cref="ReportStream"/>.
        /// </summary>
        public IDeviceEndpoint Endpoint { protected set; get; }

        /// <summary>
        /// The raw device endpoint report stream.
        /// </summary>
        public IDeviceEndpointStream ReportStream { protected set; get; }

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
        public bool Connected
        {
            protected set
            {
                connected = value;
                ConnectionStateChanged?.Invoke(this, Connected);
            }
            get => connected;
        }

        /// <summary>
        /// Invoked when <see cref="Connected"/> is changed.
        /// </summary>
        public event EventHandler<bool> ConnectionStateChanged;

        protected virtual bool Initialize()
        {
            try
            {
                ReportStream = Endpoint.Open();
                return true;
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                return false;
            }
        }

        protected virtual void Start()
        {
            if (!initialized)
                initialized = Initialize();

            if (initialized)
                workerThread.Start();
        }

        protected void Main()
        {
            try
            {
                Connected = true;
                while (Connected)
                {
                    var data = ReportStream.Read();
                    if (Parser.Parse(data) is T report)
                        OnReport(report);

                    // We create a clone of the report to avoid data being modified on the tablet debugger.
                    if (RawClone && RawReport != null && Parser.Parse(data) is T debugReport)
                        OnRawReport(debugReport);
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
                Connected = false;
            }
        }

        protected virtual void OnReport(T report) => Report?.Invoke(this, report);
        protected virtual void OnRawReport(T report) => RawReport?.Invoke(this, report);

        public void Dispose()
        {
            Connected = false;
            ReportStream?.Dispose();
        }
    }
}
