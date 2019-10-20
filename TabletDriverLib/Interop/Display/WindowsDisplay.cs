using System.Linq;

namespace TabletDriverLib.Interop.Display
{
    public class WindowsDisplay : IDisplay
    {
        private static Native.Windows.DisplayInfoCollection Displays => Native.Windows.GetDisplays();
        
        public float Width
        {
            get => Displays.Sum(d => d.ScreenWidth);
        }

        public float Height 
        {
            get => Displays.Sum(d => d.ScreenHeight);
        }
    }
}