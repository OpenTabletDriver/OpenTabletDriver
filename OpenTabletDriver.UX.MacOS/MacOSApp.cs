using System.Diagnostics;

namespace OpenTabletDriver.UX.MacOS
{
    public class MacOSApp : App
    {
        private Process? _daemon;

        public MacOSApp(string[] args) : base(Eto.Platforms.Mac64, args)
        {
        }

        protected override bool EnableTray => true;

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

        public override void Exit(int code)
        {
            if (_daemon != null)
            {
                _daemon.Exited -= HandleDaemonExited;
                _daemon.Close();
            }

            base.Exit(code);
        }

        private void HandleDaemonExited(object? o, EventArgs eventArgs)
        {
            _daemon = null;
            StartDaemon();
        }
    }
}
