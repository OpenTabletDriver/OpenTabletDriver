using Microsoft.Win32.SafeHandles;

namespace OpenTabletDriver.Native.Windows
{
    public class SafeFileHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public SafeFileHandle()
            : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            return Windows.CloseHandle(handle);
        }
    }
}
