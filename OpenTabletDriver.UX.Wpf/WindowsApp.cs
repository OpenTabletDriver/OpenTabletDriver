using System.Diagnostics;
using Eto;
using Eto.Forms;

namespace OpenTabletDriver.UX.Wpf
{
    public class WindowsApp : App
    {
        private Process? _daemon;

        public WindowsApp(string[] args) : base(Platforms.Wpf, args)
        {
        }

        protected override bool EnableTray => true;

        protected override void ApplyStyles()
        {
            Style.Add<Scrollable>(null, c =>
            {
                c.Border = BorderType.None;
            });
        }

        protected override void OpenInternal(string uri, bool isDirectory)
        {
            if (isDirectory)
            {
                Process.Start("explorer", $"\"{uri.Replace("&", "^&")}\"");
            }
            else
            {
                var startInfo = new ProcessStartInfo("cmd", $"/c start {uri.Replace("&", "^&")}")
                {
                    CreateNoWindow = true
                };
                Process.Start(startInfo);
            }
        }

        public override void StartDaemon()
        {
            if (Instance.Exists("OpenTabletDriver.Daemon"))
                return;

            var daemonPath = Path.Join(AppContext.BaseDirectory, "OpenTabletDriver.Daemon.exe");
            if (File.Exists(daemonPath))
            {
                _daemon = new Process
                {
                    StartInfo = new ProcessStartInfo(daemonPath)
                    {
                        CreateNoWindow = true
                    },
                    EnableRaisingEvents = true
                };
                _daemon.Exited += HandleDaemonExited;
                _daemon.Start();
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

        private void HandleDaemonExited(object? sender, EventArgs e)
        {
            _daemon = null;
            StartDaemon();
        }
    }
}
