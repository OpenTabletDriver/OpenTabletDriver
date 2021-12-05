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
            GLib.ExceptionManager.UnhandledException += ShowUnhandledException;
            App.Run(Eto.Platforms.Gtk, args);
        }

        private static void ShowUnhandledException(UnhandledExceptionArgs args)
        {
            var exception = args.ExceptionObject as Exception;
            Plugin.Log.Exception(exception);
            exception.ShowMessageBox();
        }
    }
}
