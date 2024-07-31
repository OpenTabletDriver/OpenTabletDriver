namespace OpenTabletDriver.Native.Linux.Timers
{
    public enum TimerFlag
    {
        Default,
        AbsoluteTime,
        TFD_NONBLOCK = 2048 // open timer file descriptor as non-blocking
    }
}
