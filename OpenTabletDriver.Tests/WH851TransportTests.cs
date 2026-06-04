using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.InteropServices;
using OpenTabletDriver.Devices;
using OpenTabletDriver.Tablet;
using Xunit;

namespace OpenTabletDriver.Tests
{
    public sealed class WH851TransportTests
    {
        [Fact]
        public void TransportPriorityPrefersUsbOverStaleBluetooth()
        {
            var usb = CreateDevice("usb", transportGroup: "Gaomon WH851", transportPriority: "20");
            var bluetooth = CreateDevice("bluetooth", transportGroup: "Gaomon WH851", transportPriority: "10");

            var filteredDevices = InvokeTransportPriorityFilter(usb, bluetooth);

            var device = Assert.Single(filteredDevices);
            Assert.Same(usb, device);
        }

        [Fact]
        public void TransportPriorityDoesNotFilterUngroupedDevices()
        {
            var first = CreateDevice("first", transportGroup: null, transportPriority: null);
            var second = CreateDevice("second", transportGroup: null, transportPriority: null);

            var filteredDevices = InvokeTransportPriorityFilter(first, second);

            Assert.Equal(2, filteredDevices.Length);
            Assert.Contains(first, filteredDevices);
            Assert.Contains(second, filteredDevices);
        }

        [Fact]
        public void BluetoothLeHidChildMatcherAcceptsWindowsBthLeDeviceHardwareId()
        {
            Assert.True(IsBluetoothLeHidChild(
                new[]
                {
                    @"BTHLEDevice\{00001812-0000-1000-8000-00805f9b34fb}_Dev_VID&02256c_PID&8251_REV&0001",
                    @"BTHLEDevice\{00001812-0000-1000-8000-00805f9b34fb}_Dev_VID&02256c_PID&8251",
                    @"BTHLEDevice\{00001812-0000-1000-8000-00805f9b34fb}_LOCALMFG&0002"
                },
                vendorId: 0x256c,
                productId: 0x8251));
        }

        [Fact]
        public void BluetoothLeHidChildMatcherAcceptsWindowsHidCollectionHardwareId()
        {
            Assert.True(IsBluetoothLeHidChild(
                new[]
                {
                    @"HID\{00001812-0000-1000-8000-00805f9b34fb}_Dev_VID&02256c_PID&8251_REV&0001&Col01",
                    @"HID\{00001812-0000-1000-8000-00805f9b34fb}_Dev_VID&02256c_PID&8251&Col01"
                },
                vendorId: 0x256c,
                productId: 0x8251));
        }

        [Fact]
        public void BluetoothLeHidChildMatcherRejectsDifferentDevice()
        {
            Assert.False(IsBluetoothLeHidChild(
                new[]
                {
                    @"BTHLEDevice\{00001812-0000-1000-8000-00805f9b34fb}_Dev_VID&02046d_PID&b386_REV&0008"
                },
                vendorId: 0x256c,
                productId: 0x8251));
        }

        [Fact]
        public void BluetoothGattClientCharacteristicConfigurationValueMatchesWindowsSdkLayout()
        {
            var descriptorValueType = typeof(Driver).Assembly.GetType(
                "OpenTabletDriver.Devices.WindowsBluetoothBackend.WindowsBluetoothGattNative+BTH_LE_GATT_DESCRIPTOR_VALUE"
            );
            Assert.NotNull(descriptorValueType);

            Assert.Equal(80, Marshal.SizeOf(descriptorValueType!));
            Assert.Equal(0, Marshal.OffsetOf(descriptorValueType!, "DescriptorType").ToInt32());
            Assert.Equal(4, Marshal.OffsetOf(descriptorValueType!, "DescriptorUuid").ToInt32());
            Assert.Equal(24, Marshal.OffsetOf(descriptorValueType!, "IsSubscribeToNotification").ToInt32());
            Assert.Equal(25, Marshal.OffsetOf(descriptorValueType!, "IsSubscribeToIndication").ToInt32());
            Assert.Equal(72, Marshal.OffsetOf(descriptorValueType!, "DataSize").ToInt32());
        }

        private static ImmutableArray<InputDevice> InvokeTransportPriorityFilter(params InputDevice[] devices)
        {
            var method = typeof(Driver).GetMethod("FilterByTransportPriority", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(method);

            return (ImmutableArray<InputDevice>)method!.Invoke(null, new object[] { devices.ToImmutableArray() })!;
        }

        private static bool IsBluetoothLeHidChild(IReadOnlyList<string> hardwareIds, int vendorId, int productId)
        {
            var type = typeof(Driver).Assembly.GetType("OpenTabletDriver.Devices.WindowsBluetoothBackend.WindowsBluetoothGattDeviceInterfaceEnumerator");
            Assert.NotNull(type);

            var method = type!.GetMethod("IsBluetoothLeHidChild", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(method);

            return (bool)method!.Invoke(null, new object[] { hardwareIds, vendorId, productId })!;
        }

        private static InputDevice CreateDevice(string path, string? transportGroup, string? transportPriority)
        {
            var attributes = new Dictionary<string, string>();
            if (transportGroup is not null)
                attributes["TransportGroup"] = transportGroup;
            if (transportPriority is not null)
                attributes["TransportPriority"] = transportPriority;

            var identifier = new DeviceIdentifier
            {
                VendorID = 0x256c,
                ProductID = path == "bluetooth" ? 0x8251 : 0x2003,
                InputReportLength = 12,
                OutputReportLength = 0,
                ReportParser = typeof(PassthroughReportParser).FullName!,
                Attributes = attributes
            };

            var configuration = new TabletConfiguration
            {
                Name = "Gaomon WH851",
                DigitizerIdentifiers = new List<DeviceIdentifier> { identifier }
            };

            var endpoint = new InputDeviceEndpoint(new DummyDriver(), new FakeEndpoint(path, identifier), configuration, identifier);
            return new InputDevice(configuration, endpoint, null);
        }

        private sealed class DummyDriver : IDriver
        {
            public event EventHandler<InputDevice>? InputDeviceAdded;
            public event EventHandler<InputDevice>? InputDeviceRemoved;
            public ImmutableArray<InputDevice> InputDevices => ImmutableArray<InputDevice>.Empty;
            public IReportParser<IDeviceReport> GetReportParser(DeviceIdentifier identifier) => new PassthroughReportParser();
            public void ScanDevices()
            {
                _ = InputDeviceAdded;
                _ = InputDeviceRemoved;
            }
        }

        private sealed class FakeEndpoint : IDeviceEndpoint
        {
            private readonly DeviceIdentifier _identifier;

            public FakeEndpoint(string devicePath, DeviceIdentifier identifier)
            {
                DevicePath = devicePath;
                _identifier = identifier;
            }

            public int ProductID => _identifier.ProductID;
            public int VendorID => _identifier.VendorID;
            public int InputReportLength => (int)(_identifier.InputReportLength ?? 0);
            public int OutputReportLength => (int)(_identifier.OutputReportLength ?? 0);
            public int FeatureReportLength => 0;
            public string? Manufacturer => "GAOMON";
            public string? ProductName => "WH851";
            public string? FriendlyName => "Gaomon WH851";
            public string? SerialNumber => "WH851";
            public string DevicePath { get; }
            public bool CanOpen => true;
            public IDictionary<string, string>? DeviceAttributes => null;
            public IDeviceEndpointStream? Open() => null;
            public string? GetDeviceString(byte index) => null;
            public bool IsSibling(IDeviceEndpoint other) => other.SerialNumber == SerialNumber;
        }
    }
}
