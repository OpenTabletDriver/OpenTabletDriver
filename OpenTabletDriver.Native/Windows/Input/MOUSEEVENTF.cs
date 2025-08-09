namespace OpenTabletDriver.Native.Windows.Input
{
    public enum MOUSEEVENTF : uint
    {
        ABSOLUTE = 0x8000,
        MOVE = 0x0001,
        VIRTUALDESK = 0x4000,
        LEFTDOWN = 0x0002,
        LEFTUP = 0x0004,
        MIDDLEDOWN = 0x0020,
        MIDDLEUP = 0x0040,
        RIGHTDOWN = 0x0008,
        RIGHTUP = 0x0010,
        XDOWN = 0x0080,
        XUP = 0x0100,
        MOVE_NOCOALESCE = 0x2000
    }
}
