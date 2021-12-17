using System;
using Eto.Forms;
using GLib;

namespace OpenTabletDriver.UX.Gtk
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            App.Run(Eto.Platforms.Gtk, args);
        }
    }
}
