using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Devices;

namespace OpenTabletDriver.Devices
{
    public class RootHub : IRootHub
    {
        public RootHub()
        {
            var rootHubs = from type in Assembly.GetExecutingAssembly().DefinedTypes
                           where type.GetCustomAttribute<RootHubAttribute>() != null
                           select type;

            hubs = rootHubs.Select(t => (IRootHub)Activator.CreateInstance(t)).ToList();
            ForceEnumeration();

            foreach (var hub in hubs)
            {
                hub.DevicesChanged += (sender, eventArgs) =>
                {
                    Log.Debug(sender.GetType().Name, $"Changes: {eventArgs.Changes.Count()}, Add: {eventArgs.Additions.Count()}, Remove: {eventArgs.Removals.Count()}");
                    OnDevicesChanged(sender, eventArgs);
                };
            }
        }

        private readonly object syncObject = new();
        private List<IRootHub> hubs;
        private List<IDeviceEndpoint> oldEndpoints;
        private readonly List<IDeviceEndpoint> endpoints = new();
        private long version;
        private int currentlyDebouncing;

        public event EventHandler<DevicesChangedEventArgs> DevicesChanged;

        private async void OnDevicesChanged(object sender, DevicesChangedEventArgs eventArgs)
        {
            var lastVersion = Interlocked.Increment(ref version);
            if (Interlocked.Increment(ref currentlyDebouncing) == 1)
            {
                // This event is the first of a potential sequence of events, copy old endpoint list
                oldEndpoints = new List<IDeviceEndpoint>(endpoints);
            }

            lock (syncObject)
            {
                endpoints.RemoveAll(e => eventArgs.Removals.Contains(e));
                endpoints.AddRange(eventArgs.Additions);
            }

            await Task.Delay(20);

            if (version == lastVersion)
            {
                Log.Write(nameof(RootHub), "Invoking DevicesChanged", LogLevel.Debug);
                DevicesChanged?.Invoke(this, new DevicesChangedEventArgs(oldEndpoints, endpoints));
            }
            else
            {
                Log.Write(nameof(RootHub), $"Debounced {sender.GetType().Name}'s DevicesChanged event");
            }

            Interlocked.Decrement(ref currentlyDebouncing);
        }

        public IEnumerable<IDeviceEndpoint> GetDevices()
        {
            return endpoints;
        }

        private void ForceEnumeration()
        {
            endpoints.Clear();
            endpoints.AddRange(hubs.SelectMany(h => h.GetDevices()));
        }

        public IEnumerable<IRootHub> GetHubs()
        {
            return hubs;
        }

        public void RegisterRootHub(IRootHub rootHub)
        {
            hubs.Add(rootHub);
            oldEndpoints = new List<IDeviceEndpoint>(endpoints);
            ForceEnumeration();
            DevicesChanged?.Invoke(this, new DevicesChangedEventArgs(endpoints, oldEndpoints));
            oldEndpoints = null;
        }

        public void RemoveRootHub(IRootHub rootHub)
        {
            hubs.Remove(rootHub);
            oldEndpoints = new List<IDeviceEndpoint>(endpoints);
            ForceEnumeration();
            DevicesChanged?.Invoke(this, new DevicesChangedEventArgs(endpoints, oldEndpoints));
            oldEndpoints = null;
        }

        public static readonly IRootHub Current = new RootHub();
    }
}