using System.Threading.Tasks;

#nullable enable

namespace OpenTabletDriver.Desktop
{
    public interface IApplicationLifetime
    {
        Task Run(string[] args);
    }
}
