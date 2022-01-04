using System;

namespace OpenTabletDriver.UX.MacOS
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            App.Run(Eto.Platforms.Mac64, args);
        }
    }
}
