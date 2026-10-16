using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using OpenTabletDriver.Devices;
using OpenTabletDriver.Plugin.Components;
using OpenTabletDriver.Plugin.Devices;
using Xunit;

namespace OpenTabletDriver.Tests
{
    public class DeviceHubSnapshotTests
    {
        [Fact]
        public void EventData_RemainsStableWhenSourceListsChange()
        {
            var removed = Endpoint("removed");
            var added = Endpoint("added");
            var previous = new List<IDeviceEndpoint> { removed };
            var current = new List<IDeviceEndpoint> { added };
            var changes = new DevicesChangedEventArgs(previous, current);

            previous.Clear();
            current.Clear();

            Assert.Equal(new[] { added }, changes.Additions);
            Assert.Equal(new[] { removed }, changes.Removals);
        }

        [Fact]
        public void GetDevices_ReturnsSnapshotAcrossDeviceNotifications()
        {
            var first = Endpoint("first");
            var child = new FakeHub(first);
            var root = CreateRoot(child);
            var snapshot = root.GetDevices();

            child.SetDevices(Endpoint("second"));

            Assert.Equal(new[] { first }, snapshot);
        }

        [Fact]
        public async Task Notification_CanBeEnumeratedWhileAnotherNotificationUpdatesDevices()
        {
            var child = new FakeHub();
            var root = CreateRoot(child);
            var first = Endpoint("first");
            var second = Endpoint("second");
            var replacement = Endpoint("replacement");
            var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var handled = false;
            root.DevicesChanged += (_, changes) =>
            {
                if (handled)
                    return;
                handled = true;
                try
                {
                    using var additions = changes.Additions.GetEnumerator();
                    Assert.True(additions.MoveNext());
                    // Device notifications update the hub synchronously before their debounce delay.
                    child.SetDevices(replacement);
                    Assert.True(additions.MoveNext());
                    Assert.False(additions.MoveNext());
                    Assert.Equal(new[] { first, second }, changes.Current);
                    completion.SetResult();
                }
                catch (Exception ex)
                {
                    completion.SetException(ex);
                }
            };

            child.SetDevices(first, second);
            await completion.Task.WaitAsync(TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void ConnectingHub_ReportsItsDevicesAsAdditions()
        {
            var existing = Endpoint("existing");
            var added = Endpoint("added");
            var root = CreateRoot(new FakeHub(existing));
            DevicesChangedEventArgs? received = null;
            root.DevicesChanged += (_, changes) => received = changes;

            root.ConnectDeviceHub(new FakeHub(added));

            Assert.NotNull(received);
            Assert.Equal(new[] { added }, received.Additions);
            Assert.Empty(received.Removals);
            Assert.Equal(new[] { existing }, received.Previous);
            Assert.Equal(new[] { existing, added }, received.Current);
        }

        [Fact]
        public void DisconnectingHub_ReportsItsDevicesAsRemovals()
        {
            var existing = Endpoint("existing");
            var removed = Endpoint("removed");
            var child = new FakeHub(removed);
            var root = CreateRoot(new FakeHub(existing), child);
            DevicesChangedEventArgs? received = null;
            root.DevicesChanged += (_, changes) => received = changes;

            root.DisconnectDeviceHub(child);

            Assert.NotNull(received);
            Assert.Empty(received.Additions);
            Assert.Equal(new[] { removed }, received.Removals);
            Assert.Equal(new[] { existing, removed }, received.Previous);
            Assert.Equal(new[] { existing }, received.Current);
        }

        private static RootHub CreateRoot(params IDeviceHub[] children)
        {
            var provider = Substitute.For<IDeviceHubsProvider>();
            provider.DeviceHubs.Returns(children);
            return new RootHub(provider);
        }

        private static IDeviceEndpoint Endpoint(string path)
        {
            var endpoint = Substitute.For<IDeviceEndpoint>();
            endpoint.DevicePath.Returns(path);
            return endpoint;
        }

        private sealed class FakeHub(params IDeviceEndpoint[] initialDevices) : IDeviceHub
        {
            private IDeviceEndpoint[] devices = initialDevices;

            public event EventHandler<DevicesChangedEventArgs>? DevicesChanged;

            public IEnumerable<IDeviceEndpoint> GetDevices() => devices;

            public void SetDevices(params IDeviceEndpoint[] updatedDevices)
            {
                var previous = devices;
                devices = updatedDevices;
                DevicesChanged?.Invoke(this, new DevicesChangedEventArgs(previous, devices));
            }
        }
    }
}
