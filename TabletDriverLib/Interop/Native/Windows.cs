using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TabletDriverLib.Component;

namespace TabletDriverLib.Interop.Native
{
    internal static class Windows
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }

            public static explicit operator Point(POINT p)
            {
                return new Point(p.X, p.Y);
            }

            public static explicit operator POINT(Point p)
            {
                return new POINT((int)p.X, (int)p.Y);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MonitorInfo
        {
            public uint size;
            public Rect monitor;
            public Rect work;
            public uint flags;
        }

        #region Cursor

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(out POINT lpPoint);

        #endregion

        #region Display

        public delegate bool MonitorEnumDelegate(IntPtr hMonitor,IntPtr hdcMonitor,ref Rect lprcMonitor, IntPtr dwData);

        [DllImport("user32.dll")]
        public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumDelegate lpfnEnum, IntPtr dwData);

        [DllImport("user32.dll")]
        public static extern bool GetMonitorInfo(IntPtr hmon, ref MonitorInfo mi);

        public class DisplayInfo
        {
            public string Availability { get; set; }
            public float ScreenHeight { get; set; }
            public float ScreenWidth { get; set; }
            public Rect MonitorArea { get; set; }
            public Rect WorkArea { get; set; }
        }

        public class DisplayInfoCollection : List<DisplayInfo>
        {
        }

        public static DisplayInfoCollection GetDisplays()
        {
            DisplayInfoCollection col = new DisplayInfoCollection();

            EnumDisplayMonitors( IntPtr.Zero, IntPtr.Zero,
                delegate (IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor,  IntPtr dwData)
                {
                    MonitorInfo mi = new MonitorInfo();
                    mi.size = (uint)Marshal.SizeOf(mi);
                    bool success = GetMonitorInfo(hMonitor, ref mi);
                    if (success)
                    {
                        DisplayInfo di = new DisplayInfo();
                        di.ScreenWidth = mi.monitor.right - mi.monitor.left;
                        di.ScreenHeight = mi.monitor.bottom - mi.monitor.top;
                        di.MonitorArea = mi.monitor;
                        di.WorkArea = mi.work;
                        di.Availability = mi.flags.ToString();
                        col.Add(di);
                    }
                return true;
                }, IntPtr.Zero );
            return col;
        }

        #endregion
    }
}