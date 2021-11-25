using System.Threading.Tasks;
using Octokit;

namespace OpenTabletDriver.Desktop.Updater
{
    public interface IUpdater
    {
        Task<bool> CheckForUpdates(bool forced = true);
        Task<Release> GetRelease();
        Task InstallUpdate();
    }
}