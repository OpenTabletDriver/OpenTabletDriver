namespace OpenTabletDriver.Native.Windows.USB
{
    public enum DescriptorType : byte
    {
        Undefined,
        Device,
        Configuration,
        StringDescriptor,
        Interface,
        Endpoint,
        InterfacePower = 0x08,
        OTG,
        Debug,
        InterfaceAssociation,
        BOS = 0x15,
        DeviceCapability,
        HID = 0x21,
        Report,
        Physical,
        SuperSpeedUSBEndpointCompanion = 0x48,
        SuperSpeedPlusIsochronousEndpointCompanion,
    }

    public static class DescriptorTypeExtensions
    {
        public static ushort WithIndex(this DescriptorType descriptorType, int index) =>
            (ushort)(((ushort)descriptorType << 8) | index);
    }
}
