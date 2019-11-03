namespace NativeLib.Windows
{
    public class DisplayInfo
    {
        public string Availability { get; set; }
        public float ScreenHeight { get; set; }
        public float ScreenWidth { get; set; }
        public Rect MonitorArea { get; set; }
        public Rect WorkArea { get; set; }
    }
}