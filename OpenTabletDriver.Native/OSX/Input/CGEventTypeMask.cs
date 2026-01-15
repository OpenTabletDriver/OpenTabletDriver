using System;

namespace OpenTabletDriver.Native.OSX.Input
{
	[Flags]
	public enum CGEventTypeMask : ulong
	{
		Null = 0,
		LeftMouseDown = 1UL << (int)CGEventType.kCGEventLeftMouseDown,
		RightMouseDown = 1UL << (int)CGEventType.kCGEventRightMouseDown,
		MouseMoved = 1UL << (int)CGEventType.kCGEventMouseMoved,
		LeftMouseDragged = 1UL << (int)CGEventType.kCGEventLeftMouseDragged,
		RightMouseDragged = 1UL << (int)CGEventType.kCGEventRightMouseDragged,
		TabletPointer = 1UL << (int)CGEventType.kCGEventTabletPointer,
		TabletProximity = 1UL << (int)CGEventType.kCGEventTabletProximity,
	}
}