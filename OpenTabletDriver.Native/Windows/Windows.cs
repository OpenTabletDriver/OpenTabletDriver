using System;
using System.IO;
using System.Runtime.InteropServices;
using OpenTabletDriver.Native.Windows.Input;
using OpenTabletDriver.Native.Windows.Timers;

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

        [DllImport("Shcore.dll")]
        public static extern int SetProcessDpiAwareness(int awareness);

        #endregion

        #region Input

        [DllImport("user32.dll")]
        public static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);

        #endregion

        #region Timers

        public delegate void TimerCallback(uint uTimerID, uint uMsg, UIntPtr dwUser, UIntPtr dw1, UIntPtr dw2);

        [DllImport("winmm.dll", SetLastError = true)]
        public static extern uint timeGetDevCaps(ref TimeCaps timeCaps, uint sizeTimeCaps);

        [DllImport("winmm.dll", SetLastError = true)]
        public static extern uint timeBeginPeriod(uint uMilliseconds);

        [DllImport("winmm.dll", SetLastError = true)]
        public static extern uint timeEndPeriod(uint uMilliseconds);

        [DllImport("winmm.dll", SetLastError = true)]
        public static extern uint timeSetEvent(uint msDelay, uint msResolution, TimerCallback handler,
            IntPtr data, EventType eventType);

        [DllImport("winmm.dll", SetLastError = true)]
        public static extern uint timeKillEvent(uint timerEventId);

        #endregion

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern SafeFileHandle CreateFile(
            [MarshalAs(UnmanagedType.LPWStr)] string filename,
            FileAccess access,
            FileShare share,
            IntPtr securityAttributes,
            FileMode creationDisposition,
            FileAttributes flagsAndAttributes,
            IntPtr templateFile);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hHandle);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetProcessInformation(IntPtr hProcess, ProcessInformationClass processInformationClass, IntPtr processInformation, int processInformationLength);

        public enum ProcessInformationClass
        {
            ProcessMemoryPriority,
            ProcessMemoryExhaustionInfo,
            ProcessAppMemoryInfo,
            ProcessInPrivateInfo,
            ProcessPowerThrottling,
            ProcessReservedValue1,
            ProcessTelemetryCoverageInfo,
            ProcessProtectionLevelInfo,
            ProcessLeapSecondInfo,
            ProcessMachineTypeInfo,
            ProcessInformationClassMax
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PowerThrottlingState
        {
            public uint Version;
            public uint ControlMask;
            public uint StateMask;

            public static PowerThrottlingState Create()
            {
                return new PowerThrottlingState
                {
                    Version = 1,
                };
            }
        }

        public enum PowerThrottlingStateMask
        {
            ExecutionSpeed = 0x1,
            IgnoreTimerResolution = 0x4,
        }
    }
}
