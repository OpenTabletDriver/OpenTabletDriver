namespace OpenTabletDriver.Daemon.Interop.Environment
{
    public class MacOSEnvironmentHandler : EnvironmentHandler
    {
        public override void Open(string path)
        {
            Exec("open", $"\"{path}\"");
        }
    }
}
