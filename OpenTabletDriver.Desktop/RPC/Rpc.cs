using System;
using System.IO;
using OpenTabletDriver.Desktop.Contracts;
using StreamJsonRpc;

namespace OpenTabletDriver.Desktop.RPC
{
    public static class Rpc
    {
        [ThreadStatic]
        private static PipeMessageHandler? s_handler;

        internal static PipeMessageHandler GetRpcHandler(Stream stream) => s_handler ??=
            new LengthHeaderMessageHandler(stream, stream,
                new NerdbankMessagePackFormatter { TypeShapeProvider = Witness.GeneratedTypeShapeProvider });
    }
}
