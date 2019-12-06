using System;
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
                    if (display.MonitorArea.left < 0)
                    {
                        value += Math.Abs(display.MonitorArea.right - display.MonitorArea.left);
                    }
                    else
                    {
                        var offset = value - display.MonitorArea.left;
                        value += display.MonitorWidth - offset;
                    }
                }
                return value;
            }
        }

        public float Height 
        {
            get
            {
                int result = 0;
                foreach (var display in Displays)
                {
                    if (display.MonitorArea.top < 0)
                    {
                        result += Math.Abs(display.MonitorArea.bottom - display.MonitorArea.top);
                    }
                    else
                    {
                        var offset = result - display.MonitorArea.top;
                        result += display.MonitorHeight - offset;
                    }
                }
                return result;
            }
        }
    }
}