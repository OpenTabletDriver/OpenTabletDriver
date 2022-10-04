using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Components;

namespace OpenTabletDriver.Devices
{
    /// <summary>
    /// A reflection-sourced composite root hub.
    /// </summary>
    [PublicAPI]
    public class RootHub : ICompositeDeviceHub
    {
        public RootHub(IDeviceHubsProvider hubsProvider)
        {
            var internalHubs = hubsProvider.DeviceHubs.ToHashSet();
            _hubs = new HashSet<IDeviceHub>(internalHubs);

            var internalLegacyHubs = hubsProvider.LegacyDeviceHubs.ToHashSet();
            _legacyHubs = new HashSet<ILegacyDeviceHub>(internalLegacyHubs);

            ForceEnumeration();

            foreach (var hub in _hubs)
            {
                HookDeviceNotification(hub);
            }

            Log.Write(nameof(RootHub), $"Initialized internal child hubs: {string.Join(", ", _hubs.Select(h => h.GetType().Name))}", LogLevel.Debug);
        }

        private readonly object _syncObject = new object();
        private readonly HashSet<IDeviceHub> _hubs;
        private readonly HashSet<ILegacyDeviceHub> _legacyHubs;
        private List<IDeviceEndpoint>? _oldEndpoints;
        private readonly List<IDeviceEndpoint> _endpoints = new List<IDeviceEndpoint>();
        private long _version;
        private int _currentlyDebouncing;
        private IServiceProvider? _serviceProvider;

        public event EventHandler<DevicesChangedEventArgs>? DevicesChanged;

        public IEnumerable<IDeviceHub> DeviceHubs => _hubs;

        public IEnumerable<ILegacyDeviceHub> LegacyDeviceHubs => _legacyHubs;

        private IEnumerable<Uri> _legacyPorts;

        public IEnumerable<Uri> LegacyPorts => _legacyPorts;

        public static RootHub WithProvider(IServiceProvider provider)
        {
            return new RootHub(provider.GetRequiredService<IDeviceHubsProvider>())
                .RegisterServiceProvider(provider);
        }

        public IEnumerable<IDeviceEndpoint> GetDevices()
        {
            return _endpoints;
        }

        public void ConnectDeviceHub<T>() where T : IDeviceHub
        {
            if (_serviceProvider == null)
                throw new InvalidOperationException("Cannot instantiate device hubs without a service provider");

            ConnectDeviceHub(ActivatorUtilities.CreateInstance<T>(_serviceProvider!));
        }

        public void ConnectDeviceHub(IDeviceHub rootHub)
        {
            Log.Write(nameof(RootHub), $"Connecting hub: {rootHub.GetType().Name}", LogLevel.Debug);
            if (_hubs.Add(rootHub))
            {
                CommitHubChange();
                HookDeviceNotification(rootHub);
            }
            else
            {
                Log.Write(nameof(RootHub), $"Connection failed, {rootHub.GetType().Name} is already connected", LogLevel.Debug);
            }
        }

        public void DisconnectDeviceHub<T>() where T : IDeviceHub
        {
            foreach (var hub in DeviceHubs.Where(static t => t.GetType().IsAssignableTo(typeof(T))))
            {
                DisconnectDeviceHub(hub);
            }
        }

        public void DisconnectDeviceHub(IDeviceHub rootHub)
        {
            Log.Write(nameof(RootHub), $"Disconnecting hub: {rootHub.GetType().Name}", LogLevel.Debug);
            if (_hubs.Remove(rootHub))
            {
                CommitHubChange();
                UnhookDeviceNotification(rootHub);
            }
            else
            {
                Log.Write(nameof(RootHub), $"Disconnection failed, {rootHub.GetType().Name} is not a connected hub", LogLevel.Debug);
            }
        }

        private async void OnDevicesChanged(object? sender, DevicesChangedEventArgs eventArgs)
        {
            var lastVersion = Interlocked.Increment(ref _version);
            if (Interlocked.Increment(ref _currentlyDebouncing) == 1)
            {
                // This event is the first of a potential sequence of events, copy old endpoint list
                _oldEndpoints = new List<IDeviceEndpoint>(_endpoints);
            }

            lock (_syncObject)
            {
                _endpoints.RemoveAll(e => eventArgs.Removals.Contains(e, DevicesChangedEventArgs.Comparer));
                _endpoints.AddRange(eventArgs.Additions);
            }

            await Task.Delay(20);

            if (_version == lastVersion)
            {
                Log.Write(nameof(RootHub), "Invoking DevicesChanged", LogLevel.Debug);
                DevicesChanged?.Invoke(this, new DevicesChangedEventArgs(_oldEndpoints, _endpoints));
            }
            else
            {
                Log.Write(nameof(RootHub), $"Debounced {sender?.GetType().Name}'s DevicesChanged event");
            }

            Interlocked.Decrement(ref _currentlyDebouncing);
        }

        private void HookDeviceNotification(IDeviceHub rootHub)
        {
            rootHub.DevicesChanged += HandleHubDeviceNotification;
        }

        private void UnhookDeviceNotification(IDeviceHub rootHub)
        {
            rootHub.DevicesChanged -= HandleHubDeviceNotification;
        }

        private void HandleHubDeviceNotification(object? sender, DevicesChangedEventArgs eventArgs)
        {
            if (eventArgs.Changes.Any())
            {
                Log.Debug(sender?.GetType().Name ?? nameof(RootHub), $"Changes: {eventArgs.Changes.Count()}, Add: {eventArgs.Additions.Count()}, Remove: {eventArgs.Removals.Count()}");
                OnDevicesChanged(sender, eventArgs);
            }
        }

        private void CommitHubChange()
        {
            _oldEndpoints = new List<IDeviceEndpoint>(_endpoints);
            ForceEnumeration();
            DevicesChanged?.Invoke(this, new DevicesChangedEventArgs(_endpoints, _oldEndpoints));
            _oldEndpoints = null;
        }

        private void ForceEnumeration()
         {
            _endpoints.Clear();
            _endpoints.AddRange(_hubs.SelectMany(h => h.GetDevices()));
            _legacyPorts = _legacyHubs.Where(h => h.CanEnumeratePorts).SelectMany(h => h.EnumeratePorts());
        }

        private RootHub RegisterServiceProvider(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
            return this;
        }
    }
}
