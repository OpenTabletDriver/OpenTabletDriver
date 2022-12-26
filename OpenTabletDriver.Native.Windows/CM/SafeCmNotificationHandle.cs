using Microsoft.Win32.SafeHandles;
using OpenTabletDriver.Native.Windows.CM;
using static OpenTabletDriver.Native.Windows.CfgMgr32;

namespace OpenTabletDriver.Devices.WinUSB
{
    public class SafeCmNotificationHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public SafeCmNotificationHandle()
            : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            return CM_Unregister_Notification(handle) == CR.SUCCESS;
        }
    }
}
