using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Timers;

namespace OpenTabletDriver.UX
{
    public class DaemonWatchdog: IDisposable
    {
        public event EventHandler DaemonExited;

        private Process daemonProcess = new Process
        {
            StartInfo = 
            {
                FileName = FileName,
                Arguments = Arguments,
                CreateNoWindow = true
            }
        };

        private Timer watchdogTimer = new Timer(1000);

        // This will break if dotnet isn't in PATH
        private static string FileName => "dotnet";
        private static string Arguments => Path.Join(Directory.GetCurrentDirectory(), "OpenTabletDriver.Daemon.dll");
        internal static bool CanExecute => File.Exists(Arguments);

        public void Start()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                daemonProcess.StartInfo.FileName = "OpenTabletDriver.Daemon.exe";
                daemonProcess.StartInfo.Arguments = null;
            }

            this.daemonProcess.Start();
            this.watchdogTimer.Start();
            this.watchdogTimer.Elapsed += (sender, e) =>
            {
                this.daemonProcess.Refresh();
                if (this.daemonProcess.HasExited)
                    DaemonExited?.Invoke(this, new EventArgs());
            };
        }
        
        public void Stop()
        {
            this.watchdogTimer?.Stop();
            this.daemonProcess?.Kill();
        }

        public void Dispose()
        {
            Stop();
            this.watchdogTimer?.Dispose();
            this.daemonProcess?.Dispose();
        }
    }
}