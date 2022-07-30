namespace OpenTabletDriver.Native.OSX.Input
{
    public enum CGMouseEventSubtype : ulong
    {
        kCGEventMouseSubtypeDefault = 0,
        kCGEventMouseSubtypeTabletPoint = 1,
        kCGEventMouseSubtypeTabletProximity = 2
    }
}
