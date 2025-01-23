using System.Threading.Tasks;

namespace OpenTabletDriver.Daemon.Library
{
    public interface IApplicationLifetime
    {
        Task Run(string[] args);
    }
}
