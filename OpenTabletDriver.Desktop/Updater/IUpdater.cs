using System.Threading.Tasks;
using Octokit;

namespace OpenTabletDriver.Desktop.Updater
{
    public interface IUpdater
    {
        Task<Release> GetLatest();
        Task<bool> CheckForUpdate();
        Task Update();
        Task Install(Release release);
        Task Install(Release release, string targetDir);
        Task<string> Download(Release release);
        ReleaseAsset GetAsset(Release release);
    }
}