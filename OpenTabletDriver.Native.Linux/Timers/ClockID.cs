namespace OpenTabletDriver.Native.Linux.Timers
{
    public enum ClockID
    {
        RealTime,
        Monotonic,
        ProcessCPUTimeID,
        ThreadCPUTimeID,
        MonotonicRaw,
        RealTimeCourse,
        MonotonicCourse,
        BootTime,
        RealTimeAlarm,
        BootTimeAlarm,
        SGICycle,
        TAI
    }
}
