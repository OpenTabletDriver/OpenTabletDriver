using System;
using System.Diagnostics;
using System.IO;
using System.Timers;
using OpenTabletDriver.Native;

namespace OpenTabletDriver.UX
{
    public class DaemonWatchdog: IDisposable
    {
        public event EventHandler DaemonExited;

        private Process daemonProcess = new Process
        {
            StartInfo = startInfo
        };

        private Timer watchdogTimer = new Timer(1000);

        private static ProcessStartInfo startInfo => SystemInfo.CurrentPlatform switch
        {
            RuntimePlatform.Windows => new ProcessStartInfo
            {
                FileName = Path.Join(Directory.GetCurrentDirectory(), "OpenTabletDriver.Daemon.exe"),
                Arguments = "",
                WorkingDirectory = Directory.GetCurrentDirectory(),
                CreateNoWindow = true
            },
            _ => new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = Path.Join(Directory.GetCurrentDirectory(), "OpenTabletDriver.Daemon.dll"),

            }
        };

        public static bool CanExecute =>
            File.Exists(startInfo.FileName) || 
            File.Exists(startInfo.Arguments);

        public void Start()
        {
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