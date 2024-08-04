namespace OpenTabletDriver.Native.Linux.Timers
{
    public enum ClockID
    {
        RealTime = 0,
        Monotonic = 1,
        ProcessCPUTimeID = 2,
        ThreadCPUTimeID = 3,
        MonotonicRaw = 4,
        RealTimeCourse = 5,
        MonotonicCourse = 6,
        BootTime = 7,
        RealTimeAlarm = 8,
        BootTimeAlarm = 9,
        SGICycle = 10,
        TAI = 11
    }
}
