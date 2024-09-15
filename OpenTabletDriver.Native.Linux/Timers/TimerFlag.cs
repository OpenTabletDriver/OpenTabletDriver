namespace OpenTabletDriver.Native.Linux.Timers
{
    public enum TimerFlag
    {
        Default = 0,
        AbsoluteTime = 1 << 0,
        CancelOnSet = 1 << 1
    }
}
