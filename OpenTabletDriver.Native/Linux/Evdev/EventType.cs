namespace OpenTabletDriver.Native.Linux.Evdev
{
    public enum EventType : uint
    {
        EV_SYN = 0x00,
        EV_KEY = 0x01,
        EV_REL = 0x02,
        EV_ABS = 0x03,
        INPUT_PROP_DIRECT = 0x01
    }
}
