using OpenTabletDriver.Plugin.Logging;

namespace OpenTabletDriver.Contracts
{
    public interface ILogServer
    {
        void Post(LogMessage message);
    }
}