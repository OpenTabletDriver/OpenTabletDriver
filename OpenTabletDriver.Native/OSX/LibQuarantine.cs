using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.OSX
{
    public static class LibQuarantine
    {
        private const string QtLib = "/usr/lib/system/libquarantine.dylib";

        [DllImport(QtLib)]
        public static extern int responsibility_get_pid_responsible_for_pid(int pid);
    }
}
