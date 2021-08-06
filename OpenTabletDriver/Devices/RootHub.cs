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
                           where type.GetCustomAttribute<SupportedPlatformAttribute>()?.IsCurrentPlatform ?? true
                           select type;

            internalHubs = rootHubs.Select(t => (IRootHub)Activator.CreateInstance(t)).ToHashSet();
            hubs = new HashSet<IRootHub>(internalHubs);
            ForceEnumeration();

            foreach (var hub in hubs)
            {
                HookDeviceNotification(hub);
            }

            Log.Write(nameof(RootHub), $"Initialized internal child hubs: {string.Join(", ", hubs.Select(h => h.GetType().Name))}", LogLevel.Debug);
        }

        private readonly object syncObject = new();
        private readonly HashSet<IRootHub> internalHubs;
        private readonly HashSet<IRootHub> hubs;
        private List<IDeviceEndpoint> oldEndpoints;
        private readonly List<IDeviceEndpoint> endpoints = new();
        private long version;
        private int currentlyDebouncing;

        public event EventHandler<DevicesChangedEventArgs> DevicesChanged;

        public static readonly RootHub Current = new RootHub();

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

        public IEnumerable<IRootHub> GetHubs()
        {
            return hubs;
        }

        public void RegisterRootHub(IRootHub rootHub)
        {
            Log.Write(nameof(RootHub), $"Registering hub: {rootHub.GetType().Name}", LogLevel.Debug);
            if (hubs.Add(rootHub))
            {
                CommitHubChange();
                HookDeviceNotification(rootHub);
            }
            else
            {
                Log.Write(nameof(RootHub), $"Registry failed, {rootHub.GetType().Name} is already registered", LogLevel.Debug);
            }
        }

        public void UnregisterRootHub(IRootHub rootHub)
        {
            Log.Write(nameof(RootHub), $"Unregistering hub: {rootHub.GetType().Name}", LogLevel.Debug);
            if (hubs.Remove(rootHub))
            {
                CommitHubChange();
                UnhookDeviceNotification(rootHub);
            }
            else
            {
                Log.Write(nameof(RootHub), $"Unregistry failed, {rootHub.GetType().Name} is not a registered hub", LogLevel.Debug);
            }
        }

        private void HookDeviceNotification(IRootHub rootHub)
        {
            rootHub.DevicesChanged += HandleHubDeviceNotification;
        }

        private void UnhookDeviceNotification(IRootHub rootHub)
        {
            rootHub.DevicesChanged -= HandleHubDeviceNotification;
        }

        private void HandleHubDeviceNotification(object sender, DevicesChangedEventArgs eventArgs)
        {
            if (eventArgs.Changes.Any())
            {
                Log.Debug(sender.GetType().Name, $"Changes: {eventArgs.Changes.Count()}, Add: {eventArgs.Additions.Count()}, Remove: {eventArgs.Removals.Count()}");
                OnDevicesChanged(sender, eventArgs);
            }
        }

        private void CommitHubChange()
        {
            oldEndpoints = new List<IDeviceEndpoint>(endpoints);
            ForceEnumeration();
            DevicesChanged?.Invoke(this, new DevicesChangedEventArgs(endpoints, oldEndpoints));
            oldEndpoints = null;
        }

        private void ForceEnumeration()
        {
            endpoints.Clear();
            endpoints.AddRange(hubs.SelectMany(h => h.GetDevices()));
        }
    }
}