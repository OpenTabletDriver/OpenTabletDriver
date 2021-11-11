using System.Threading.Tasks;

namespace OpenTabletDriver.Desktop.Updater
{
    public interface IUpdater
    {
        Task<bool> HasUpdate { get; }
        Task InstallUpdate(string targetDir);
    }
}