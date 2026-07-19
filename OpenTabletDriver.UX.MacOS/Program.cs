using System;

namespace OpenTabletDriver.UX.MacOS
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            if (PermissionHelper.HasPermissions())
            {
                App.Run("OpenTabletDriver.UX.MacOS.Platform, OpenTabletDriver.UX.MacOS", args);
            }
        }
    }
}
