using System.IO;
using static OpenTabletDriver.Daemon.Contracts.FileUtilities;

namespace OpenTabletDriver.Daemon.Interop
{
    public class WindowsAppInfo : Contracts.AppInfo
    {
        public WindowsAppInfo()
        {
            AppDataDirectory = GetExistingPathOrLast(AppDataDirectory, Path.Join(ProgramDirectory, "userdata"), "$LOCALAPPDATA\\OpenTabletDriver");
        }
    }
}
