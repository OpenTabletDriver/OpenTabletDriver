using System.Diagnostics;
using Eto;
using Eto.Forms;

namespace OpenTabletDriver.UX.MacOS
{
    public class MacOSApp : App
    {
        private Process? _daemon;

        public MacOSApp(string[] args) : base(Platforms.Mac64, args)
        {
        }

        protected override bool EnableTray => true;

        protected override void ApplyStyles()
        {
            Style.Add<GroupBox>(null, c =>
            {
                c.Padding = 5;
            });
        }

        protected override void OpenInternal(string uri, bool isDirectory)
        {
            Process.Start("open", $"\"{uri}\"");
        }

        public override void StartDaemon()
        {
            if (Instance.Exists("OpenTabletDriver.Daemon"))
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
    }
}
