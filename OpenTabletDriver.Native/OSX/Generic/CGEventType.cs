namespace OpenTabletDriver.Native.OSX
{
    public enum CGEventType : uint
    {
        kCGEventNull = 0,
        kCGEventLeftMouseDown = 1,
        kCGEventLeftMouseUp = 2,
        kCGEventRightMouseDown = 3,
        kCGEventRightMouseUp = 4,
        kCGEventMouseMoved = 5,
        kCGEventLeftMouseDragged = 6,
        kCGEventRightMouseDragged = 7,
        kCGEventKeyDown = 10,
        kCGEventKeyUp = 11,
        kCGEventOtherMouseDown = 16,
        kCGEventOtherMouseUp = 17,
        kCGEventOtherMouseDragged = 27 // Possibly incorrect, based on other docs
    }
}