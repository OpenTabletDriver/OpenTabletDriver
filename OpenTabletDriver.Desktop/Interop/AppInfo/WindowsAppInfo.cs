using System.IO;

namespace OpenTabletDriver.Desktop.Interop.AppInfo
{
    using static FileUtilities;

    public class WindowsAppInfo : AppInfo
    {
        public WindowsAppInfo()
        {
            AppDataDirectory = GetExistingPathOrLast(Path.Join(ProgramDirectory, "userdata"), "$LOCALAPPDATA\\OpenTabletDriver");
        }
    }
}
