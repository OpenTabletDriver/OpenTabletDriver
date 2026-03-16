using System;

namespace OpenTabletDriver.UX.Gtk
{
    internal class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            App.Run(Eto.Platforms.Gtk, args);
        }
    }
}
