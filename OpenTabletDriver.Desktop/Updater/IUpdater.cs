using System.Threading.Tasks;

namespace OpenTabletDriver.Desktop.Updater
{
    public interface IUpdater
    {
        Task<bool> CheckForUpdates();
        Task<UpdateInfo?> GetInfo();
        Task InstallUpdate();
    }
}
