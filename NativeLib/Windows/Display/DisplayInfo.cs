namespace NativeLib.Windows
{
    public class DisplayInfo
    {
        public DisplayInfo(Rect monitorArea, Rect workingArea)
        {
            MonitorArea = monitorArea;
            WorkingArea = workingArea;
        }

        public Rect MonitorArea { private set; get; }
        public Rect WorkingArea { private set; get; }

        public int Width => MonitorArea.right - MonitorArea.left;
        public int Height => MonitorArea.bottom - MonitorArea.top;
        public int Top => MonitorArea.top;
        public int Left => MonitorArea.left;
        public int Bottom => MonitorArea.bottom;
        public int Right => MonitorArea.right;

        public override string ToString()
        {
            return string.Format("{0}x{1}@{2},{3}", Width, Height, Top, Left);
        }
    }
}