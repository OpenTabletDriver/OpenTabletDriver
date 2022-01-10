using Microsoft.Win32.SafeHandles;
using static OpenTabletDriver.Native.Windows.WinUsb;

namespace OpenTabletDriver.Native.Windows.USB
{
    public class SafeWinUsbInterfaceHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public SafeWinUsbInterfaceHandle() : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            WinUsb_Free(handle);
            return true;
        }
    }
}
