using System.Collections.Generic;
using NativeLib.Windows;

namespace TabletDriverLib.Interop.Display
{
    using static Windows;

    public class WindowsDisplay : IDisplay
    {
        private static IEnumerable<DisplayInfo> Displays => GetDisplays();
        
        public float Width
        {
            get
            {
                int value = 0;
                foreach (var display in Displays)
                {
                    var offset = value - display.MonitorArea.left;
                    value += display.MonitorWidth - offset;
                }
                return value;
            }
        }

        public float Height 
        {
            get
            {
                int value = 0;
                foreach (var display in Displays)
                {
                    var offset = value - display.MonitorArea.top;
                    value += display.MonitorHeight - offset;
                }
                return value;
            }
        }
    }
}