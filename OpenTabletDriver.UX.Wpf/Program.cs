using System;
using System.Security.Principal;
using Eto.Forms;

namespace OpenTabletDriver.UX.Wpf
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new(identity);
            if (principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                _ = new Application(Eto.Platforms.Wpf);
                MessageBox.Show("OpenTabletDriver should not be run with administrator privileges.\n" + 
                    "Some features may not work as intended.\n" +
                    "If you did not manually set OpenTabletDriver to run with administrator privileges please enable UAC to resolve this.", MessageBoxType.Warning);
                Eto.Platform.AllowReinitialize = true;
            }

            App.Run(Eto.Platforms.Wpf, args);
        }
    }
}
