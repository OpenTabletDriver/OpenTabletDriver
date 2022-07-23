using System.Diagnostics;

namespace OpenTabletDriver.UX.MacOS
{
    public class MacOSApp : App
    {
        public MacOSApp(string[] args) : base(Eto.Platforms.Mac64, args)
        {
        }

        protected override void OpenInternal(string uri, bool isDirectory)
        {
            Process.Start("open", $"\"{uri}\"");
        }

        public override bool CanUpdate => true;

        public override void StartDaemon()
        {
            if (Instance.Exists("OpenTabletDriver.Daemon"))
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

                daemon.Exited += (_, _) =>
                {
                    StartDaemon();
                };
            }
        }
    }
}
