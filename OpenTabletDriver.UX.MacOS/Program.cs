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
                MessageBox.Show("OpenTabletDriver must be installed to the Applications folder.");
                return;
            }

            App.Run(Eto.Platforms.Mac64, args);
        }
    }
}
