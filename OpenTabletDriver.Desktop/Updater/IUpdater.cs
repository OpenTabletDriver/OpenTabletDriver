using System.Threading.Tasks;
using Octokit;

namespace OpenTabletDriver.Desktop.Updater
{
    public interface IUpdater
    {
        Task<bool> CheckForUpdates();
        Task<Release> GetRelease();
        Task InstallUpdate();
    }
}
