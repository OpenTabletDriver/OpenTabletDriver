using System.Diagnostics;

namespace OpenTabletDriver.UX.Wpf
{
    public class WindowsApp : App
    {
        public WindowsApp(string[] args) : base(Eto.Platforms.Wpf, args)
        {
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
            // TODO: Add daemon watchdog
            throw new NotImplementedException();
        }
    }
}
