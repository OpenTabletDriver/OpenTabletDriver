using System.IO;
using StreamJsonRpc;

namespace OpenTabletDriver.Daemon.Contracts.RPC.Messages
{
    public class MessageHandler : NewLineDelimitedMessageHandler
    {
        public MessageHandler(Stream stream)
            : base(stream, stream, new MessageFormatter(Json.Utilities.Converters))
        {
        }
    }
}
