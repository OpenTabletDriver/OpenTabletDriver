namespace OpenTabletDriver.Daemon.Interop.Environment
{
    public class LinuxEnvironmentHandler : EnvironmentHandler
    {
        public override void Open(string path)
        {
            Exec("xdg-open", $"\"{path}\"");
        }
    }
}
