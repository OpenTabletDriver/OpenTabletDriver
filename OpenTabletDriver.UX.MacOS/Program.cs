using System;

namespace OpenTabletDriver.UX.MacOS
{
    internal class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            if (PermissionHelper.HasPermissions())
            {
                App.Run(Eto.Platforms.Mac64, args);
            }
        }
    }
}
