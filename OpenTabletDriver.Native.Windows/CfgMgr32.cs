using System;
using System.Runtime.InteropServices;
using OpenTabletDriver.Devices.WinUSB;
using OpenTabletDriver.Native.Windows.CM;

namespace OpenTabletDriver.Native.Windows
{
    public static class CfgMgr32
    {
        public unsafe delegate int CM_NOTIFY_CALLBACK(IntPtr hNotify, IntPtr Context, CM_NOTIFY_ACTION Action, CM_NOTIFY_EVENT_DATA* EventData, int EventDataSize);

        [DllImport("cfgmgr32.dll", CharSet = CharSet.Unicode)]
        public static extern CR CM_Register_Notification(in CM_NOTIFY_FILTER pFilter, IntPtr pContext, CM_NOTIFY_CALLBACK pCallback, out SafeCmNotificationHandle pNotifyContext);

        [DllImport("cfgmgr32.dll", CharSet = CharSet.Unicode)]
        public static extern CR CM_Unregister_Notification(IntPtr NotifyContext);
    }
}
