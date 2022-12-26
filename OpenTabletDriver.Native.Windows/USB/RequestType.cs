using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.Windows.USB
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RequestType
    {
        public RequestType(RequestDirection direction, RequestInternalType type, RequestRecipient recipient)
        {
            Raw = 0;
            Direction = direction;
            Type = type;
            Recipient = recipient;
        }

        public byte Raw;

        public RequestDirection Direction
        {
            get => (RequestDirection)((Raw >> 7) & 0b1);
            set
            {
                Raw &= 0b0111_1111;
                Raw |= (byte)(((byte)value & 0b1) << 7);
            }
        }

        public RequestInternalType Type
        {
            get => (RequestInternalType)((Raw >> 5) & 0b11);
            set
            {
                Raw &= 0b1001_1111;
                Raw |= (byte)(((byte)value & 0b11) << 5);
            }
        }

        public RequestRecipient Recipient
        {
            get => (RequestRecipient)(Raw & 0b0001_1111);
            set
            {
                Raw &= 0b1110_0000;
                Raw |= (byte)((byte)value & 0b0001_1111);
            }
        }
    }
}
