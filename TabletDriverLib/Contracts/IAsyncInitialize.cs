using System.Threading.Tasks;

namespace TabletDriverLib.Contracts
{
    public interface IAsyncInitialize
    {
        Task InitializeAsync();
    }
}