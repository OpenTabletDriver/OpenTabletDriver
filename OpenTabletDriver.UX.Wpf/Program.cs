using System;

namespace OpenTabletDriver.UX.Wpf
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            App.Run(Eto.Platforms.Wpf, args);
        }
    }
}
