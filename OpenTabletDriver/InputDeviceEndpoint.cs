using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using OpenTabletDriver.Devices;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver
{
    /// <summary>
    /// An input device endpoint.
    /// </summary>
    [PublicAPI]
    public sealed class InputDeviceEndpoint : IDisposable
    {
        private InputDeviceState _state;
        private IDeviceEndpointStream? _reportStream;
        private Thread? _reportThread;

        public InputDeviceEndpoint(IDriver driver, IDeviceEndpoint device, TabletConfiguration configuration, DeviceIdentifier identifier)
        {
            ArgumentNullException.ThrowIfNull(driver);
            ArgumentNullException.ThrowIfNull(device);
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(identifier);

            Endpoint = device;
            Parser = driver.GetReportParser(identifier);
            Configuration = configuration;
            Identifier = identifier;
        }

        /// <summary>
        /// The tablet configuration referring to this device.
        /// </summary>
        public TabletConfiguration Configuration { get; }

        /// <summary>
        /// The device identifier used to detect this device.
        /// </summary>
        public DeviceIdentifier Identifier { get; }

        public IDeviceEndpoint Endpoint { get; }
        public IReportParser<IDeviceReport> Parser { get; }

        public InputDeviceState State
        {
            get => _state;
            set
            {
                if (_state == value)
                    return;
                _state = value;
                StateChanged?.Invoke(this, value);
            }
        }
        public Exception? Exception { get; private set; }

        public bool CloneReport { get; set; }

        public event EventHandler<InputDeviceState>? StateChanged;
        public event EventHandler<IDeviceReport>? ReportParsed;
        public event EventHandler<IDeviceReport>? ReportCloned;

        public void Initialize(bool process)
        {
            if (!process && State == InputDeviceState.Normal)
            {
                State = InputDeviceState.Uninitialized;
                _reportThread!.Join();
                _reportThread = null;
                return;
            }

            if (State == InputDeviceState.Normal || State == InputDeviceState.Disconnected)
                return;

            Exception = null;
            var reportStream = Endpoint.Open();
            if (reportStream is null)
                return;

            Log.Debug("Device", $"Using report parser type '{Identifier.ReportParser}'");

            foreach (byte index in Identifier.InitializationStrings ?? Enumerable.Empty<byte>())
            {
                Endpoint.GetDeviceString(index);
                Log.Debug("Device", $"Initialized string index {index}");
            }

            foreach (var report in Identifier.FeatureInitReport ?? Enumerable.Empty<byte[]>())
            {
                if (report.Length == 0)
                    continue;

                try
                {
                    reportStream!.SetFeature(report);
                    Log.Debug("Device", "Set device feature: " + BitConverter.ToString(report));
                }
                catch
                {
                    Log.Write("Device", "Failed to set device feature: " + BitConverter.ToString(report), LogLevel.Warning);
                }
            }

            foreach (var report in Identifier.OutputInitReport ?? Enumerable.Empty<byte[]>())
            {
                if (report.Length == 0)
                    continue;

                try
                {
                    reportStream!.Write(report);
                    Log.Debug("Device", "Set device output: " + BitConverter.ToString(report));
                }
                catch
                {
                    Log.Write("Device", "Failed to set device output: " + BitConverter.ToString(report), LogLevel.Warning);
                }
            }

            _reportStream = reportStream;
            State = InputDeviceState.Normal;

            _reportThread = new Thread(ProcessReports)
            {
                Name = $"OpenTabletDriver Device Reader",
                Priority = ThreadPriority.AboveNormal,
            };
            _reportThread.Start();
        }

        private void SetException(string message, Exception exception)
        {
            State = InputDeviceState.Faulted;
            Exception = exception;
            Log.Write("Device", message, LogLevel.Error, notify: true);
            Log.Exception(exception);
        }

        private void ProcessReports()
        {
            while (State == InputDeviceState.Normal)
            {
                if (!TryRead(out var data))
                    return;

                if (!TryParse(data, out var report))
                    return;

                // Create a clone even if processing the report and pushing it
                // down the pipeline created an exception.
                TryProcess(report);

                // We create a clone of the report to avoid data being modified on the tablet debugger.
                // No need to wrap in a try catch since this is guaranteed to not throw
                if (CloneReport && Parser.Parse(data) is IDeviceReport clonedReport)
                    ReportCloned?.Invoke(this, clonedReport);
            }
        }

        private bool TryRead([NotNullWhen(true)] out byte[]? data)
        {
            try
            {
                data = _reportStream!.Read();
                return true;
            }
            catch (ObjectDisposedException)
            {
                State = InputDeviceState.Disposed;
                data = null;
                return false;
            }
            catch (IOException ioEx) when (ioEx.Message is "I/O disconnected." or "Operation failed after some time.")
            {
                State = InputDeviceState.Disconnected;
                data = null;
                return false;
            }
            catch (Exception e)
            {
                SetException($"Failed to read report from '{Endpoint.GetDetailedName()}'", e);
                data = null;
                return false;
            }
        }

        private bool TryParse(byte[] data, out IDeviceReport? report)
        {
            try
            {
                report = Parser.Parse(data);
                return true;
            }
            catch (Exception e)
            {
                report = null;
                SetException($"Failed to parse report from '{Endpoint.GetDetailedName()}'", e);
                return false;
            }
        }

        private bool TryProcess(IDeviceReport? report)
        {
            if (report is null)
                return false;

            try
            {
                ReportParsed?.Invoke(this, report);
                return true;
            }
            catch (Exception e)
            {
                SetException($"Failed to process report from '{Endpoint.GetDetailedName()}'", e);
                return false;
            }
        }

        public void Dispose()
        {
            if (State == InputDeviceState.Disposed)
                return;

            State = InputDeviceState.Disposed;
            _reportStream?.Dispose();
        }
    }
}
