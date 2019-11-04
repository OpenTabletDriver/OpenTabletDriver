using System.Collections.Generic;
using System.Linq;
using NativeLib.Windows;

namespace TabletDriverLib.Interop.Display
{
    using static Windows;

    public class WindowsDisplay : IDisplay
    {
        private static IEnumerable<DisplayInfo> Displays => GetDisplays();
        
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