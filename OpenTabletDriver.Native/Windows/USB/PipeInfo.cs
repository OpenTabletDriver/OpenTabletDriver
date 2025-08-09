using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.Windows.USB
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PipeInfo
    {
        public PipeType PipeType;
        public byte PipeID;
        private byte sizePacket0;
        private byte sizePacket1;
        public ushort MaximumPacketSize => (ushort)(sizePacket1 | (sizePacket0 << 8));
        public byte Interval;
    }
}
