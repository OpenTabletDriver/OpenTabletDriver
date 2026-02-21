using System.Runtime.InteropServices;

// TODO: remove nullable disable
#nullable disable

namespace OpenTabletDriver.Native.OSX
{
    static public class LibQuarantine
    {
        private const string QtLib = "/usr/lib/system/libquarantine.dylib";

        [DllImport(QtLib)]
        static public extern int responsibility_get_pid_responsible_for_pid(int pid);
    }
}
