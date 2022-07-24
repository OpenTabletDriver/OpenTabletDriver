using System.Threading.Tasks;

namespace OpenTabletDriver.Desktop
{
    public interface IApplicationLifetime
    {
        Task Run(string[] args);
    }
}
