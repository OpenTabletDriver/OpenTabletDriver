namespace OpenTabletDriver.Native.Windows.USB
{
    public enum StandardRequestCode : byte
    {
        GetStatus,
        ClearFeature,
        SetFeature = 3,
        SetAddress = 5,
        GetDescriptor,
        SetDescriptor,
        GetConfiguration,
        SetConfiguration,
        GetInterface,
        SetInterface,
        SynchFrame,
        SetEncryption,
        GetEncryption,
        SetHandshake,
        GetHandshake,
        SetConnection,
        SetSecurityData,
        GetSecurityData,
        SetWUsbData,
        LoopbackDataWrite,
        LoopbackDataRead,
        SetInterfaceDs,
        SetSel = 48,
        SetIsochDelay
    }
}
