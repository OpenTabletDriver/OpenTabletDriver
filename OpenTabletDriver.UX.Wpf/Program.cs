using System;
using System.Security.Principal;
using System.Threading.Tasks;
using Eto.Forms;

namespace OpenTabletDriver.UX.Wpf
{
    class Program
    {
        [STAThread]
        public static async Task Main(string[] args)
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new(identity);
            if (principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                _ = new Application(Eto.Platforms.Wpf);
                MessageBox.Show("OpenTabletDriver should not be run with administrator privileges.\n" +
                    "Some features may not work as intended, such as Plugin Manager and Tablet Debugger.\n" +
                    "If you did not manually set OpenTabletDriver to run with administrator privileges please enable UAC to resolve this.\n" +
                    "If it isn't resolved after, you are using the Administrator named account which overrides UAC.", MessageBoxType.Warning);
                Eto.Platform.AllowReinitialize = true;
            }

            await App.Run(Eto.Platforms.Wpf, args);
        }
    }
}
