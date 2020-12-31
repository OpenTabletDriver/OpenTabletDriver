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
                MessageBox.Show("Do not run OpenTabletDriver as administrator.", MessageBoxType.Error);
                Environment.Exit(1);
            }

            App.Run(Eto.Platforms.Wpf, args);
        }
    }
}
