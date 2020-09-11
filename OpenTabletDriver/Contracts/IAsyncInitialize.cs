using System.Threading.Tasks;

namespace OpenTabletDriver.Contracts
{
    public interface IAsyncInitialize
    {
        Task InitializeAsync();
    }
}