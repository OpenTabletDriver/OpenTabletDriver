using System.Threading.Tasks;

#nullable enable

namespace OpenTabletDriver.Desktop.Updater
{
    public interface IUpdater
    {
        Task<bool> CheckForUpdates();
        Task<UpdateInfo?> GetInfo();
        Task InstallUpdate();
    }
}
