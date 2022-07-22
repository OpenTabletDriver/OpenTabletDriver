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
            // TODO: Add daemon watchdog
            throw new NotImplementedException();
        }
    }
}
