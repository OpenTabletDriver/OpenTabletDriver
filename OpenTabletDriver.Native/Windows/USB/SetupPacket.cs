using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.Windows.USB
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SetupPacket
    {
        public RequestType bmRequestType;
        public byte bRequest;
        public ushort wValue;
        public ushort wIndex;
        public ushort wLength;

        public static SetupPacket MakeGetConfiguration()
        {
            return new SetupPacket()
            {
                bmRequestType = new RequestType(RequestDirection.DeviceToHost, RequestInternalType.Standard, RequestRecipient.Device),
                bRequest = (byte)StandardRequestCode.GetConfiguration,
                wLength = 1
            };
        }

        public static SetupPacket MakeSetConfiguration(byte configuration)
        {
            return new SetupPacket()
            {
                bmRequestType = new RequestType(RequestDirection.HostToDevice, RequestInternalType.Standard, RequestRecipient.Device),
                bRequest = (byte)StandardRequestCode.SetConfiguration,
                wIndex = configuration
            };
        }

        public static SetupPacket MakeGetDescriptor(RequestInternalType type, RequestRecipient recipient, DescriptorType descriptorType, ushort index, ushort size, ushort otherIndex = 0)
        {
            return new SetupPacket()
            {
                bmRequestType = new RequestType(RequestDirection.DeviceToHost, type, recipient),
                bRequest = (byte)StandardRequestCode.GetDescriptor,
                wValue = descriptorType.WithIndex(index),
                wIndex = otherIndex,
                wLength = size
            };
        }

        public static SetupPacket MakeGetStringDescriptor(ushort index, ushort langId = 0)
        {
            return MakeGetDescriptor(
                RequestInternalType.Standard,
                RequestRecipient.Device,
                DescriptorType.StringDescriptor,
                index,
                StringDescriptor.MaxSize,
                langId);
        }
    }
}
