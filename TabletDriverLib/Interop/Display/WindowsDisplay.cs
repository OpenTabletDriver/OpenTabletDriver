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

        public Point Position => new Point(0, 0);

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