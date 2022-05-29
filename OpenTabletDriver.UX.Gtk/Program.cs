using System;
using System.Threading.Tasks;

namespace OpenTabletDriver.UX.Gtk
{
    class Program
    {
        [STAThread]
        public static async Task Main(string[] args)
        {
            await App.Run(Eto.Platforms.Gtk, args);
        }
    }
}
