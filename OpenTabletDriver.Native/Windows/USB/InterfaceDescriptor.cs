using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.Windows.USB
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct InterfaceDescriptor
    {
        public readonly DescriptorHeader Header;
        public readonly byte bInterfaceNumber;
        public readonly byte bAlternateSetting;
        public readonly byte bNumEndpoints;
        public readonly byte bInterfaceClass;
        public readonly byte bInterfaceSubClass;
        public readonly byte bInterfaceProtocol;
        public readonly byte iInterface;
    }
}
