using System.Diagnostics;

namespace OpenTabletDriver.UX.Wpf
{
    public class WindowsApp : App
    {
        public WindowsApp(string[] args) : base(Eto.Platforms.Wpf, args)
        {
        }

        protected override bool EnableTray => true;

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
                var daemon = new Process
                {
                    StartInfo = new ProcessStartInfo(daemonPath)
                    {
                        CreateNoWindow = true
                    },
                    EnableRaisingEvents = true
                };
                daemon.Start();

                daemon.Exited += (_, _) =>
                {
                    StartDaemon();
                };
            }
        }
    }
}
