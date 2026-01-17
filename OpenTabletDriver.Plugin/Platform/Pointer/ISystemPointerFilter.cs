namespace OpenTabletDriver.Plugin.Platform.Pointer
{
    public interface ISystemPointerFilter
    {
        bool Enabled { get; }

        void ConnectionStatusChanged(string deviceName, bool connected);
    }
}
