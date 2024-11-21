using System;
using JetBrains.Annotations;
using OpenTabletDriver.Output;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver
{
    /// <summary>
    /// A configured input device.
    /// </summary>
    [PublicAPI]
    public sealed class InputDevice : IDisposable
    {
        private InputDeviceState _state;
        private object _sync = new object();

        public InputDevice(TabletConfiguration configuration, InputDeviceEndpoint digitizer, InputDeviceEndpoint? auxiliary)
        {
            Configuration = configuration;
            Digitizer = digitizer;
            Auxiliary = auxiliary;

            hookEndpoint(Digitizer);
            hookEndpoint(Auxiliary);

            Initialize(true);

            void hookEndpoint(InputDeviceEndpoint? endpoint)
            {
                if (endpoint is null)
                    return;

                endpoint.StateChanged += (sender, state) =>
                {
                    // inhibit going from higher severity to lower severity
                    // that's going to be handled by InputDevice itself
                    if (state <= State)
                        return;

                    var oldState = State;
                    State = state;
                    if (oldState == InputDeviceState.Normal)
                    {
                        if (state == InputDeviceState.Faulted)
                        {
                            var exception = ((InputDeviceEndpoint)sender!).Exception;
                            Exception = exception;
                        }

                        // stop processing of both endpoints
                        Initialize(false);
                    }
                };
                endpoint.ReportParsed += HandleReport;
            }
        }

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

        public TabletConfiguration Configuration { get; private set; }
        public InputDeviceEndpoint Digitizer { get; }
        public InputDeviceEndpoint? Auxiliary { get; }

        public event EventHandler<InputDeviceState>? StateChanged;

        /// <summary>
        /// The active output mode at the end of the data pipeline for all data to be processed.
        /// </summary>
        public IOutputMode? OutputMode { set; get; }

        public void Initialize(bool process)
        {
            Digitizer.Initialize(process);
            Auxiliary?.Initialize(process);
        }

        private void HandleReport(object? sender, IDeviceReport? report)
        {
            lock (_sync)
            {
                // no need to try catch here, it's handled by the InputDeviceEndpoints
                OutputMode?.Read(report!);
            }
        }

        public void Dispose()
        {
            Digitizer.Dispose();
            Auxiliary?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
