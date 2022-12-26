namespace OpenTabletDriver.Native.Windows.USB
{
    public enum RequestRecipient : byte
    {
        Device,
        Interface,
        Endpoint,
        Other,
        VendorDefined = 0b0001_1111
    }
}
