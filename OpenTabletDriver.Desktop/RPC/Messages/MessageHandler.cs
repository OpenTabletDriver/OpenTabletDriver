using System.IO;
using StreamJsonRpc;

#nullable enable

namespace OpenTabletDriver.Desktop.RPC.Messages
{
    public class MessageHandler : NewLineDelimitedMessageHandler
    {
        public MessageHandler(Stream stream)
            : base(stream, stream, new MessageFormatter(Json.Utilities.Converters))
        {
        }
    }
}
