using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Devices;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Devices
{
    public class DeviceReader : IDisposable
    {
        public DeviceReader(IDeviceEndpoint endpoint, IReportParser reportParser)
        {
            Endpoint = endpoint;
            Parser = reportParser ?? throw new ArgumentNullException(nameof(reportParser));
            workerThread = new Thread(Main)
            {
                Name = "OpenTabletDriver Device Reader",
                IsBackground = true,
                Priority = ThreadPriority.AboveNormal,
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
        public IDeviceEndpointStream? ReportStream { protected set; get; }

        /// <summary>
        /// The <see cref="IReportParser"/> in which the device reports will be parsed with.
        /// </summary>
        public IReportParser Parser { private set; get; }

        /// <summary>
        /// Whether to make an extra cloned report with data left unmodified.
        /// </summary>
        public bool RawClone { set; get; }

        /// <summary>
        /// Invoked when a new report comes in from the device.
        /// </summary>
        public event RefEventHandler<DeviceReport>? Report;

        /// <summary>
        /// Invoked when a new report comes in from the device.
        /// </summary>
        /// <remarks>
        /// This will only be invoked when <see cref="RawClone"/> is set to true.
        /// This report is not meant in any way to be modified, as it is supposed to represent the original data.
        /// </remarks>
        // TODO: check if not using RefEventHandler is enough to have it duplicate?
        public event EventHandler<DeviceReport>? RawReport;

        /// <summary>
        /// Whether the device is actively emitting reports and being parsed.
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
        public event EventHandler<bool>? ConnectionStateChanged;

        [MemberNotNullWhen(true, nameof(ReportStream))]
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

        private long _allocatedBytes;

        [Conditional("DEBUG")]
        private static void spewMemoryUse(string context, ref long allocatedBytes)
        {
            var newAllocatedBytes = GC.GetAllocatedBytesForCurrentThread();
            var allocateDiff = newAllocatedBytes - allocatedBytes;
            Log.Write("mem", $"{allocateDiff}: {context}", LogLevel.Debug);

            allocatedBytes = GC.GetAllocatedBytesForCurrentThread(); // don't reuse newAllocatedBytes as logging might have changed this
        }

        protected void Main()
        {
            Span<byte> data = stackalloc byte[Endpoint.InputReportLength];
            try
            {
                Connected = true;
                while (Connected)
                {
                    _allocatedBytes = GC.GetAllocatedBytesForCurrentThread();
                    ReportStream!.Read(data);
                    var report = new DeviceReport(data);
                    spewMemoryUse("data-read", ref _allocatedBytes);

                    bool parsed = Parser.Parse(data, ref report);
                    spewMemoryUse("parse", ref _allocatedBytes);
                    Debug.Assert(parsed == report.Continue);
                    Debug.Assert(report.Parser != null, $"Parser must set {nameof(DeviceReport.Parser)} in device report, as it is used for serialization");
                    Debug.Assert(report.Parser.IsAssignableTo(typeof(IReportParser)), $"Parser must set {nameof(DeviceReport.Parser)} to a valid {nameof(IReportParser)}");

                    if (parsed)
                    {
                        if (RawClone)
                            OnRawReport(report, Parser);
                        OnReport(ref report);

                        spewMemoryUse("post-report", ref _allocatedBytes);
                    }

                    if (report.OutOfRange)
                        GC.Collect(2, GCCollectionMode.Forced);

                    report.Dispose();
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
                Log.Write("Device", $"Last report read from tablet: {data.PrettyPrintHex()}");
            }
            finally
            {
                Connected = false;
            }
        }

        protected virtual void OnReport(scoped ref DeviceReport report) => Report?.Invoke(this, ref report);
        protected virtual void OnRawReport(scoped in DeviceReport report, IReportParser parser) => RawReport?.Invoke(this, report);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _isDisposed;

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                Connected = false;
                ReportStream?.Dispose();
            }

            _isDisposed = true;
        }
    }
}
