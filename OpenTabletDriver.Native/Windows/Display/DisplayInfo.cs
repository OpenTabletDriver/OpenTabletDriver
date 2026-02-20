using OpenTabletDriver.Native.Windows.Display;

namespace OpenTabletDriver.Native.Windows
{
    public class DisplayInfo(Rect monitorArea, uint flags)
    {
        public Rect MonitorArea { private set; get; } = monitorArea;
        public bool IsPrimary { private set; get; } = ((MONITORINFOF)flags).HasFlag(MONITORINFOF.PRIMARY);

        public int Width => MonitorArea.right - MonitorArea.left;
        public int Height => MonitorArea.bottom - MonitorArea.top;
        public int Top => MonitorArea.top;
        public int Left => MonitorArea.left;
        public int Bottom => MonitorArea.bottom;
        public int Right => MonitorArea.right;

        public override string ToString()
        {
            return string.Format("{0}x{1}@{2},{3}", Width, Height, Left, Top);
        }
    }
}
