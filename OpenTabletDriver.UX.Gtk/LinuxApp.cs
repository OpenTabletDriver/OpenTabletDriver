using System.Diagnostics;

namespace OpenTabletDriver.UX.Gtk
{
    public class LinuxApp : App
    {
        public LinuxApp(string[] args) : base(Eto.Platforms.Gtk, args)
        {
        }

        protected override bool EnableTray { get; } = IsVariableSet("OTD_TRAY_ICON");

        protected override void OpenInternal(string uri, bool isDirectory)
        {
            Process.Start("xdg-open", uri);
        }

        public override void StartDaemon()
        {
            if (Instance.Exists("OpenTabletDriver.Daemon") || !IsVariableSet("OTD_UI_HOST"))
                return;

            var daemonPath = Path.Join(AppContext.BaseDirectory, "OpenTabletDriver.Daemon");
            if (File.Exists(daemonPath))
            {
                var daemon = new Process
                {
                    StartInfo = new ProcessStartInfo(daemonPath),
                    EnableRaisingEvents = true
                };
                daemon.Start();

                daemon.Exited += (_, _) => StartDaemon();
            }
        }

        private static bool IsVariableSet(string envVar)
        {
            var str = Environment.GetEnvironmentVariable(envVar);
            return str?.ToLower() is "1" or "true";
        }
    }
}
