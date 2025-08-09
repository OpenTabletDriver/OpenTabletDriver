using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.Windows.USB
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct DescriptorHeader
    {
        public readonly byte bLength;
        public readonly DescriptorType bDescriptorType;
    }
}
