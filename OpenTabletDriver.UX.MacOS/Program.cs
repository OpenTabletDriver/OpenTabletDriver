using System;
using Eto.Forms;

namespace OpenTabletDriver.UX.MacOS
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            if (baseDirectory.StartsWith("/private/var/folders"))
            {
                _ = new Application(Eto.Platforms.Mac64);
                MessageBox.Show(
                    "OpenTabletDriver cannot be run from this folder. " +
                    "Please move it to elsewhere, " +
                    "such as to the Applications folder.");
                return;
            }

            App.Run(Eto.Platforms.Mac64, args);
        }
    }
}
