using System;
using JetBrains.Annotations;
using Newtonsoft.Json;
using OpenTabletDriver.Output;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver
{
    /// <summary>
    /// A configured input device.
    /// </summary>
    [PublicAPI]
    public class InputDevice : IDisposable
    {
        public InputDevice(TabletConfiguration configuration, InputDeviceEndpoint digitizer, InputDeviceEndpoint? auxiliary)
        {
            Configuration = configuration;
            Digitizer = digitizer;
            Auxiliary = auxiliary;

            hookEndpoint(Digitizer);
            hookEndpoint(Auxiliary);

            void hookEndpoint(InputDeviceEndpoint? endpoint)
            {
                if (endpoint is null)
                    return;

                endpoint.Start();
                endpoint.ConnectionStateChanged += (sender, reading) =>
                {
                    if (_connected && !reading)
                    {
                        _connected = false;
                        Disconnected?.Invoke(this, EventArgs.Empty);
                    }
                };
                endpoint.Report += HandleReport;
            }
        }

        [JsonConstructor]
        private InputDevice()
        {
            Configuration = null!;
            Digitizer = null!;
        }

        private bool _connected = true;

        public event EventHandler<EventArgs>? Disconnected;

        public TabletConfiguration Configuration { protected set; get; }

        [JsonIgnore]
        public InputDeviceEndpoint Digitizer { get; }

        [JsonIgnore]
        public InputDeviceEndpoint? Auxiliary { get; }

        /// <summary>
        /// The active output mode at the end of the data pipeline for all data to be processed.
        /// </summary>
        [JsonIgnore]
        public IOutputMode? OutputMode { set; get; }

        private void HandleReport(object? sender, IDeviceReport? report)
        {
            OutputMode?.Read(report!);
        }

        public void Dispose()
        {
            Digitizer.Dispose();
            Auxiliary?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
