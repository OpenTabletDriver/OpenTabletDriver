using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Timers;
using Eto.Forms;
using NativeLib;

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
                CreateNoWindow = true
            }
        };

        private Timer watchdogTimer = new Timer(1000);

        internal static string FileName => SystemInfo.CurrentPlatform switch
        {
            RuntimePlatform.Windows => Path.Join(Directory.GetCurrentDirectory(), "OpenTabletDriver.Daemon.exe"),
            _                       => Path.Join(Directory.GetCurrentDirectory(), "OpenTabletDriver.Daemon")
        };

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
            this.watchdogTimer.Stop();
        }

        public void Dispose()
        {
            this.watchdogTimer?.Dispose();
            this.daemonProcess?.Dispose();
        }
    }
}