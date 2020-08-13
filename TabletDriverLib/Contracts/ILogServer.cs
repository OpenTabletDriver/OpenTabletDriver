using TabletDriverPlugin.Logging;

namespace TabletDriverLib.Contracts
{
    public interface ILogServer
    {
        void Post(LogMessage message);
    }
}