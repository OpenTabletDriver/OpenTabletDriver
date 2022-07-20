using System.IO;
using OpenTabletDriver.Desktop.RPC.Messages;
using StreamJsonRpc;

#nullable enable

namespace OpenTabletDriver.Desktop.RPC
{
    internal static class Utilities
    {
        public static JsonRpc Client(Stream stream)
        {
            return new JsonRpc(new MessageHandler(stream));
        }

        public static JsonRpc Host<T>(Stream stream, T obj)
        {
            return new JsonRpc(new MessageHandler(stream), obj);
        }
    }
}
