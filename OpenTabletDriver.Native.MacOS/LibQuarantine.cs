using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.MacOS
{
    static public class LibQuarantine
    {
        private const string QtLib = "/usr/lib/system/libquarantine.dylib";

        [DllImport(QtLib)]
        static public extern int responsibility_get_pid_responsible_for_pid(int pid);
    }
}
