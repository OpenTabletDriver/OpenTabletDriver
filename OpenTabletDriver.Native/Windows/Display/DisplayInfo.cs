using OpenTabletDriver.Native.Windows.Display;

namespace OpenTabletDriver.Native.Windows
{
    public class DisplayInfo
    {
        public DisplayInfo(Rect monitorArea, Rect workingArea, uint flags)
        {
            MonitorArea = monitorArea;
            WorkingArea = workingArea;
            IsPrimary = ((MONITORINFOF)flags).HasFlag(MONITORINFOF.PRIMARY);
        }

        public Rect MonitorArea { private set; get; }
        public Rect WorkingArea { private set; get; }

        public int Width => WorkingArea.right - WorkingArea.left;
        public int Height => WorkingArea.bottom - WorkingArea.top;
        public int Top => WorkingArea.top;
        public int Left => WorkingArea.left;
        public int Bottom => WorkingArea.bottom;
        public int Right => WorkingArea.right;
        public bool IsPrimary { private set; get; }

        public override string ToString()
        {
            return string.Format("{0}x{1}@{2},{3}", Width, Height, Left, Top);
        }
    }
}