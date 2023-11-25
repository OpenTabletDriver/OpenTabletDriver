using System.Threading.Tasks;

namespace OpenTabletDriver.Daemon
{
    public interface IApplicationLifetime
    {
        Task Run(string[] args);
    }
}
