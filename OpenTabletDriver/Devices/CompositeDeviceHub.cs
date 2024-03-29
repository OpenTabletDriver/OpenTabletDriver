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
    public class CompositeDeviceHub : ICompositeDeviceHub
    {
        public CompositeDeviceHub(IDeviceHubsProvider hubsProvider)
        {
            var internalHubs = hubsProvider.DeviceHubs.ToHashSet();
            _hubs = new HashSet<IDeviceHub>(internalHubs);

            _endpoints = GetEndpoints();

            foreach (var hub in _hubs)
            {
                HookDeviceNotification(hub);
            }

            Log.Write(nameof(CompositeDeviceHub), $"Initialized internal child hubs: {string.Join(", ", _hubs.Select(h => h.GetType().Name))}", LogLevel.Debug);
        }

        private CancellationTokenSource? _cts;
        private readonly HashSet<IDeviceHub> _hubs;
        private IDeviceEndpoint[] _endpoints;
        private IServiceProvider? _serviceProvider;

        public event EventHandler<DevicesChangedEventArgs>? DevicesChanged;

        public IEnumerable<IDeviceHub> DeviceHubs => _hubs;

        public static CompositeDeviceHub WithProvider(IServiceProvider provider)
        {
            return new CompositeDeviceHub(provider.GetRequiredService<IDeviceHubsProvider>())
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

        public void ConnectDeviceHub(IDeviceHub deviceHub)
        {
            Log.Write(nameof(CompositeDeviceHub), $"Connecting hub: {deviceHub.GetType().Name}", LogLevel.Debug);
            if (_hubs.Add(deviceHub))
            {
                HookDeviceNotification(deviceHub);
                EnqueueHubChange();
            }
            else
            {
                Log.Write(nameof(CompositeDeviceHub), $"Connection failed, {deviceHub.GetType().Name} is already connected", LogLevel.Debug);
            }
        }

        public void DisconnectDeviceHub<T>() where T : IDeviceHub
        {
            foreach (var hub in DeviceHubs.Where(static t => t.GetType().IsAssignableTo(typeof(T))))
            {
                DisconnectDeviceHub(hub);
            }
        }

        public void DisconnectDeviceHub(IDeviceHub deviceHub)
        {
            Log.Write(nameof(CompositeDeviceHub), $"Disconnecting hub: {deviceHub.GetType().Name}", LogLevel.Debug);
            if (_hubs.Remove(deviceHub))
            {
                UnhookDeviceNotification(deviceHub);
                EnqueueHubChange();
            }
            else
            {
                Log.Write(nameof(CompositeDeviceHub), $"Disconnection failed, {deviceHub.GetType().Name} is not a connected hub", LogLevel.Debug);
            }
        }

        private void HookDeviceNotification(IDeviceHub childHub)
        {
            childHub.DevicesChanged += HandleHubDeviceNotification;
        }

        private void UnhookDeviceNotification(IDeviceHub childHub)
        {
            childHub.DevicesChanged -= HandleHubDeviceNotification;
        }

        private void HandleHubDeviceNotification(object? sender, DevicesChangedEventArgs eventArgs)
        {
            Log.Debug(sender?.GetType().Name ?? nameof(CompositeDeviceHub), $"Additions: {eventArgs.Additions.Count()}, Removals: {eventArgs.Removals.Count()}");
            EnqueueHubChange();
        }

        private void EnqueueHubChange()
        {
            var newCts = new CancellationTokenSource();
            var oldCts = Interlocked.Exchange(ref _cts, newCts);
            oldCts?.Cancel();

            Task.Run(() => OnDevicesChanged(newCts.Token), newCts.Token);
        }

        private async Task OnDevicesChanged(CancellationToken ct)
        {
            // debounce for 100ms, this will be cancelled if another change is enqueued
            // as per EnqueueHubChange()
            await Task.Delay(100, ct);

            var endpoints = GetEndpoints();
            var oldEndpoints = Interlocked.Exchange(ref _endpoints, endpoints);
            Log.Write(nameof(CompositeDeviceHub), "Invoking DevicesChanged", LogLevel.Debug);
            DevicesChanged?.Invoke(this, new DevicesChangedEventArgs(oldEndpoints, endpoints));
        }

        private IDeviceEndpoint[] GetEndpoints()
        {
            return _hubs.SelectMany(h => h.GetDevices()).ToArray();
        }

        private CompositeDeviceHub RegisterServiceProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            return this;
        }
    }
}
