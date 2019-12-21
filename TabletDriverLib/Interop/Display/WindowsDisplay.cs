using System;
using System.Collections.Generic;
using System.Linq;
using NativeLib.Windows;
using TabletDriverLib.Component;

namespace TabletDriverLib.Interop.Display
{
    using static Windows;

    public class WindowsDisplay : IVirtualScreen
    {
        public WindowsDisplay()
        {
            var displays = GetDisplays();
            var left = displays.Min(d => d.Left);
            var top = displays.Min(d => d.Top);
            Position = new Point(left, top);
        }

        private IEnumerable<DisplayInfo> InternalDisplays => GetDisplays().OrderBy(e => e.Left);
        
        public float Width
        {
            get
            {
                var left = InternalDisplays.Min(d => d.Left);
                var right = InternalDisplays.Max(d => d.Right);
                return right - left;
            }
        }

        public float Height 
        {
            get
            {
                var top = InternalDisplays.Min(d => d.Top);
                var bottom = InternalDisplays.Max(d => d.Bottom);
                return bottom - top;
            }
        }

        public Point Position { private set; get; }

        public IEnumerable<IDisplay> Displays
        {
            get
            {
                foreach (var display in GetDisplays().OrderBy(e => e.Left))
                    yield return new ManualDisplay(
                        display.Width,
                        display.Height,
                        new Point(display.Top, display.Left));
            }
        }
    }
}