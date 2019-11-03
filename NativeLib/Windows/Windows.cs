using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using NativeLib.Windows.Input;

namespace NativeLib.Windows
{
    public static class Windows
    {
        private const string User32 = "user32.dll";
        
        [DllImport(User32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetCursorPos(int x, int y);

        [DllImport(User32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport(User32,CharSet=CharSet.Auto, CallingConvention=CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
        
        [DllImport(User32)]
        public static extern short GetKeyState(VirtualKeyStates nVirtKey);

        #region Display

        public delegate bool MonitorEnumDelegate(IntPtr hMonitor,IntPtr hdcMonitor,ref Rect lprcMonitor, IntPtr dwData);

        [DllImport("user32.dll")]
        public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumDelegate lpfnEnum, IntPtr dwData);

        [DllImport("user32.dll")]
        public static extern bool GetMonitorInfo(IntPtr hmon, ref MonitorInfo mi);

        public static List<DisplayInfo> GetDisplays()
        {
            List<DisplayInfo> col = new List<DisplayInfo>();

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