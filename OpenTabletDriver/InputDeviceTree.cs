using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver
{
    public class InputDeviceTree
    {
        public InputDeviceTree(TabletConfiguration configuration, IList<InputDevice> inputDevices)
        {
            Properties = configuration;
            InputDevices = inputDevices;

            foreach (var dev in InputDevices)
            {
                dev.Report += HandleReport;

                // Hook endpoint states
                dev.ConnectionStateChanged += (sender, reading) =>
                {
                    if (_connected && !reading)
                    {
                        _connected = false;
                        Disconnected?.Invoke(this, EventArgs.Empty);
                    }
                };
            }
        }

        private bool _connected = true;
        private readonly Lock _sync = new();

        public event EventHandler<EventArgs>? Disconnected;

        public TabletConfiguration Properties { protected set; get; }
        public IList<InputDevice> InputDevices { get; }

        /// <summary>
        /// The active output mode at the end of the data pipeline for all data to be processed.
        /// </summary>
        public IOutputMode? OutputMode { set; get; }

        public TabletReference CreateReference() => new TabletReference(Properties, InputDevices.Select(c => c.Identifier));

        private void HandleReport(object? sender, IDeviceReport report)
        {
            lock (_sync)
            {
                OutputMode?.Read(report);
            }
        }

        public static implicit operator TabletReference(InputDeviceTree deviceGroup) => deviceGroup.CreateReference();
    }
}
