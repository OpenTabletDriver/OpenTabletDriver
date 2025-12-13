using OpenTabletDriver.Devices.HidSharpBackend;
using OpenTabletDriver.Tests.Fakes;
using Xunit;

namespace OpenTabletDriver.Tests
{
    public class HidSharpEndpointTest
    {
        [Theory]
        [InlineData("/sys/devices/pci0000:00/0000:00:14.0/usb1/1-2/1-2:1.0/0003:056A:033C.0001/hidraw/hidraw0", "0")]
        [InlineData("/sys/devices/pci0000:00/0000:00:14.0/usb1/1-2/1-2:1.1/0003:056A:033C.0002/hidraw/hidraw1", "1")]
        [InlineData("/sys/devices/pci0000:00/0000:00:14.0/usb1/1-2/1-2:1.2/0003:056A:033C.0003/hidraw/hidraw2", "2")]
        public void DeviceAttributes_ExtractsInterfaceNumber_OnLinux(string devicePath, string expectedInterface)
        {
            var fakeDevice = new FakeHidDevice().WithDevicePath(devicePath);
            var endpoint = new HidSharpEndpoint(fakeDevice);

            var attributes = endpoint.DeviceAttributes;

            Assert.True(attributes.ContainsKey("USB_INTERFACE_NUMBER"));
            Assert.Equal(expectedInterface, attributes["USB_INTERFACE_NUMBER"]);
        }

        [Theory]
        [InlineData("/sys/devices/pci0000:00/0000:00:1d.0/usb2/2-1/2-1.4/2-1.4:1.0/0003:2833:0001.0005/hidraw/hidraw3", "0")]
        [InlineData("/sys/devices/pci0000:00/0000:00:1d.0/usb2/2-1/2-1.4/2-1.4:1.10/0003:2833:0001.0005/hidraw/hidraw3", "10")]
        [InlineData("/sys/devices/pci0000:00/0000:00:02.1/0000:05:00.0/0000:06:08.0/0000:0b:00.0/0000:0c:0c.0/0000:13:00.0/usb1/1-2/1-2.1/1-2.1.1/1-2.1.1.4/1-2.1.1.4:1.2/0003:046D:C539.000B/0003:046D:4087.0011/hidraw/hidraw16", "2")]
        [InlineData("/sys/devices/pci0000:00/0000:00:02.1/0000:05:00.0/0000:06:08.0/0000:0b:00.0/0000:0c:0c.0/0000:13:00.0/usb1/1-2/1-2.1/1-2.1.4/1-2.1.4.1/1-2.1.4.1.2/1-2.1.4.1.2:1.0/0003:256C:006D.002E/hidraw/hidraw18", "0")]
        [InlineData("/sys/devices/pci0000:00/0000:00:02.1/0000:05:00.0/0000:06:08.0/0000:0b:00.0/0000:0c:0c.0/0000:13:00.0/usb1/1-2/1-2.1/1-2.1.4/1-2.1.4.1/1-2.1.4.1.3/1-2.1.4.1.3:1.0/0003:256C:006D.002B/hidraw/hidraw19", "0")]
        [InlineData("/sys/devices/pci0000:00/0000:00:02.1/0000:05:00.0/0000:06:08.0/0000:0b:00.0/0000:0c:0c.0/0000:13:00.0/usb1/1-2/1-2.1/1-2.1.4/1-2.1.4.1/1-2.1.4.1.3/1-2.1.4.1.3:1.1/0003:256C:006D.002C/hidraw/hidraw20", "1")]
        [InlineData("/sys/devices/pci0000:00/0000:00:02.1/0000:05:00.0/0000:06:08.0/0000:0b:00.0/0000:0c:0c.0/0000:13:00.0/usb1/1-2/1-2.1/1-2.1.4/1-2.1.4.1/1-2.1.4.1.3/1-2.1.4.1.3:1.2/0003:256C:006D.002D/hidraw/hidraw21", "2")]
        public void DeviceAttributes_HandlesVariousUsbHubPaths_OnLinux(string devicePath, string expectedInterface)
        {
            var fakeDevice = new FakeHidDevice().WithDevicePath(devicePath);
            var endpoint = new HidSharpEndpoint(fakeDevice);

            var attributes = endpoint.DeviceAttributes;

            Assert.True(attributes.ContainsKey("USB_INTERFACE_NUMBER"));
            Assert.Equal(expectedInterface, attributes["USB_INTERFACE_NUMBER"]);
        }

        [Theory]
        [InlineData("/dev/hidraw0")]
        [InlineData("/sys/devices/pci0000:00/0000:00:14.0/usb1/1-2")]
        [InlineData("")]
        [InlineData("invalid/path")]
        public void DeviceAttributes_DoesNotAddInterfaceNumber_ForInvalidPath_OnLinux(string devicePath)
        {
            var fakeDevice = new FakeHidDevice().WithDevicePath(devicePath);
            var endpoint = new HidSharpEndpoint(fakeDevice);

            var attributes = endpoint.DeviceAttributes;

            Assert.False(attributes.ContainsKey("USB_INTERFACE_NUMBER"));
        }

        [Theory]
        [InlineData("/sys/devices/pci0000:00/0000:00:14.0/usb1/1-11/1-11:2.0/0003:056A:033C.0001/hidraw/hidraw0", "0")]
        [InlineData("/sys/devices/pci0000:00/0000:00:14.0/usb1/1-11/1-11:2.5/0003:056A:033C.0001/hidraw/hidraw5", "5")]
        public void DeviceAttributes_HandlesMultiDigitPortNumbers_OnLinux(string devicePath, string expectedInterface)
        {
            var fakeDevice = new FakeHidDevice().WithDevicePath(devicePath);
            var endpoint = new HidSharpEndpoint(fakeDevice);

            var attributes = endpoint.DeviceAttributes;

            Assert.True(attributes.ContainsKey("USB_INTERFACE_NUMBER"));
            Assert.Equal(expectedInterface, attributes["USB_INTERFACE_NUMBER"]);
        }
    }
}
