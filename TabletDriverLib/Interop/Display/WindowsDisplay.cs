using System;
using System.Collections.Generic;
using System.Linq;
using NativeLib.Windows;

namespace TabletDriverLib.Interop.Display
{
    using static Windows;

    public class WindowsDisplay : IDisplay
    {
        private static IEnumerable<DisplayInfo> Displays => GetDisplays().OrderBy(e => e.Left);
        
        public float Width
        {
            get
            {
                var left = Displays.Min(d => d.Left);
                var right = Displays.Max(d => d.Right);
                return right - left;
            }
        }

        public float Height 
        {
            get
            {
                var top = Displays.Min(d => d.Top);
                var bottom = Displays.Max(d => d.Bottom);
                return bottom - top;
            }
        }
    }
}