namespace NativeLib.Linux.Evdev
{
    public enum EventCode : uint
    {
        ABS_X = 0x00,
        ABS_Y = 0x01,
        BTN_LEFT = 0x110,
        BTN_RIGHT = 0x111,
        BTN_MIDDLE = 0x112,
        BTN_SIDE = 0x113,
        BTN_EXTRA = 0x114,
        BTN_FORWARD = 0x115,
        BTN_BACK = 0x116,
        SYN_REPORT = 0
    }
}