using System.Diagnostics;

namespace OpenTabletDriver.UX.Gtk
{
    public class LinuxApp : App
    {
        public LinuxApp(string[] args) : base(Eto.Platforms.Gtk, args)
        {
        }

        protected override void OpenInternal(string uri, bool isDirectory)
        {
            Process.Start("xdg-open", uri);
        }

        public override bool CanUpdate => false;

        public override void StartDaemon()
        {
            // Don't start the daemon watchdog on Linux
        }
    }
}
