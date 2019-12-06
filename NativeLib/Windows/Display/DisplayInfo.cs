namespace NativeLib.Windows
{
    public class DisplayInfo
    {
        public DisplayInfo(Rect monitorArea, Rect workingArea)
        {
            MonitorArea = monitorArea;
            WorkingArea = workingArea;
        }

        public Rect MonitorArea { get; set; }
        public Rect WorkingArea { get; set; }

        public int MonitorWidth => MonitorArea.right - MonitorArea.left;
        public int MonitorHeight => MonitorArea.bottom - MonitorArea.top;
    }
}