using System;
using System.Runtime.InteropServices;
using OpenTabletDriver.Native.Windows.Input;

namespace OpenTabletDriver.Native.Windows
{
    public static class Windows
    {
        #region Display

        public delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData);

        [DllImport("user32.dll")]
        public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumDelegate lpfnEnum, IntPtr dwData);

        [DllImport("user32.dll")]
        public static extern bool GetMonitorInfo(IntPtr hmon, ref MonitorInfoEx mi);

        [DllImport("user32.dll")]
        public static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DevMode devMode);

        [DllImport("Shcore.dll")]
        public static extern int GetDpiForMonitor(IntPtr hmon, DpiType dpiType, out uint dpiX, out uint dpiY);

        #endregion

        #region Input

        [DllImport("user32.dll")]
        public static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);

        #endregion
    }
}