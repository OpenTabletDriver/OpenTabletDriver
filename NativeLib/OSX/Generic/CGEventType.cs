namespace NativeLib.OSX
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
        kCGEventOtherMouseDown = 16, // Likely incorrect
        kCGEventOtherMouseUp = 17, // ^
    }
}