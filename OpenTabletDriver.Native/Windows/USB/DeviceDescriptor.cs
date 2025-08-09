using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.Windows.USB
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct DeviceDescriptor
    {
        public readonly DescriptorHeader Header;

        public readonly ushort bcdUSB;

        public readonly byte bDeviceClass;

        public readonly byte bDeviceSubClass;

        public readonly byte bDeviceProtocol;

        public readonly byte bMaxPacketSize0;

        public readonly ushort idVendor;

        public readonly ushort idProduct;

        public readonly ushort bcdDevice;

        public readonly byte iManufacturer;

        public readonly byte iProduct;

        public readonly byte iSerialNumber;

        public readonly byte bNumConfigurations;
    }
}
