using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Threading;
using JetBrains.Annotations;
using OpenTabletDriver.Output;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver
{
    /// <summary>
    /// A configured input device.
    /// </summary>
    [PublicAPI]
    public sealed partial class InputDevice : IDisposable
    {
        private static int _id;
        private InputDeviceState _state;

        public InputDevice(TabletConfiguration configuration, InputDeviceEndpoint digitizer, InputDeviceEndpoint? auxiliary)
        {
            // PersistentId has to be set only after all devices are known
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

        /// <summary>
        /// A unique identifier assigned to this input device. This is valid only for the current runtime.
        /// </summary>
        public int Id { get; } = Interlocked.Increment(ref _id);

        /// <summary>
        /// A unique identifier that is used in conjunction with the tablet name to identify a tablet.
        /// This can be used across reboots and driver restarts.
        /// </summary>
        public int? PersistentId { get; private set; }

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
            // no need to try catch here, it's handled by the InputDeviceEndpoints
            OutputMode?.Read(report!);
        }

        public void Dispose()
        {
            Digitizer.Dispose();
            Auxiliary?.Dispose();
            GC.SuppressFinalize(this);
        }

        internal static void AssignPersistentId(ImmutableArray<InputDevice> devices)
        {
            var persistentIdMap = new Dictionary<string, int>();

            // find the highest index for each device name
            foreach (var device in devices)
            {
                if (device.PersistentId is not int persistentId)
                    continue;

                ref var id = ref CollectionsMarshal.GetValueRefOrAddDefault(persistentIdMap, device.Configuration.Name, out _);
                if (persistentId >= id)
                    id = persistentId;
            }

            // assign incrementing persistent id to devices without one
            foreach (var device in devices)
            {
                if (device.PersistentId is not null)
                    continue;

                var index = CollectionsMarshal.GetValueRefOrAddDefault(persistentIdMap, device.Configuration.Name, out _);
                device.PersistentId = ++index;
            }
        }
    }
}
