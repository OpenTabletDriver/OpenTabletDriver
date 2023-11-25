using System.Diagnostics;

namespace OpenTabletDriver.Daemon.Interop.Environment
{
    public class WindowsEnvironmentHandler : EnvironmentHandler
    {
        public override void Open(string path)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd",
                Arguments = $"/c start {path.Replace("&", "^&")}",
                CreateNoWindow = true
            };
            Exec(startInfo);
        }

        public override void OpenFolder(string path)
        {
            Exec("explorer", $"\"{path.Replace("&", "^&")}\"");
        }
    }
}
