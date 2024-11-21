using System.Diagnostics;
using Eto;
using Eto.Drawing;
using Eto.Forms;

namespace OpenTabletDriver.UX.Gtk
{
    public class LinuxApp : App
    {
        private Process? _daemon;

        public LinuxApp(string[] args) : base(Platforms.Gtk, args)
        {
        }

        protected override bool EnableTray { get; } = IsVariableSet("OTD_TRAY_ICON");

        protected override void ApplyStyles()
        {
            Style.Add<GroupBox>(null, c =>
            {
                c.Padding = 5;
            });
            Style.Add<GroupBox>("labeled", c =>
            {
                c.BackgroundColor = SystemColors.WindowBackground;
            });

            Style.Add<Scrollable>(null, c =>
            {
                c.Border = BorderType.None;
            });
        }

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
                _daemon = new Process
                {
                    StartInfo = new ProcessStartInfo(daemonPath),
                    EnableRaisingEvents = true
                };
                _daemon.Start();

                _daemon.Exited += HandleDaemonExited;
            }
        }

        public override void StopDaemon()
        {
            if (_daemon != null)
            {
                _daemon.Exited -= HandleDaemonExited;
                _daemon.Close();
            }
        }

        public override void Exit(int code)
        {
            StopDaemon();
            base.Exit(code);
        }

        private void HandleDaemonExited(object? o, EventArgs eventArgs)
        {
            _daemon = null;
            StartDaemon();
        }

        private static bool IsVariableSet(string envVar)
        {
            var str = Environment.GetEnvironmentVariable(envVar);
            return str?.ToLower() is "1" or "true";
        }
    }
}
