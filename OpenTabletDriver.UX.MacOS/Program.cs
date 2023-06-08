using System;
using Eto.Forms;

namespace OpenTabletDriver.UX.MacOS
{
    class Program
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
