using System;
using System.IO;
using System.IO.Pipes;
using Newtonsoft.Json;
using TabletDriverPlugin.Tablet;

namespace OpenTabletDriver.Daemon
{
    internal class TabletDebuggerServer : IDisposable
    {
        public TabletDebuggerServer()
        {
            PipeServer = new NamedPipeServerStream(Identifier.ToString());
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            await PipeServer.WaitForConnectionAsync();
            StreamWriter = new StreamWriter(PipeServer);
            
            JsonWriter = new JsonTextWriter(StreamWriter);
            await JsonWriter.WriteStartArrayAsync();
        }

        public void HandlePacket(object sender, IDeviceReport report)
        {
            if (PipeServer.IsConnected)
            {
                Serializer.Serialize(JsonWriter, report);
            }
        }

        public void Dispose()
        {
            JsonWriter.WriteEndArray();

            PipeServer.Disconnect();
            PipeServer.Dispose();
            PipeServer = null;
            StreamWriter = null;
        }

        public Guid Identifier { private set; get; } = Guid.NewGuid();

        private NamedPipeServerStream PipeServer { set; get; }
        private StreamWriter StreamWriter { set; get; }
        private JsonTextWriter JsonWriter { set; get; }
        private JsonSerializer Serializer { set; get; } = new JsonSerializer
        {
            TypeNameHandling = TypeNameHandling.All
        };
    }
}