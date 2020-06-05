using System;
using System.IO;
using System.IO.Pipes;
using Newtonsoft.Json;
using TabletDriverPlugin.Tablet;


namespace OpenTabletDriverDaemon
{
    internal class TabletDebuggerServer : IDisposable
    {
        public TabletDebuggerServer(string pipeName)
        {
            PipeServer = new NamedPipeServerStream(pipeName);
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

        public NamedPipeServerStream PipeServer { private set; get; }
        private StreamWriter StreamWriter { set; get; }
        private JsonTextWriter JsonWriter { set; get; }
        private JsonSerializer Serializer { set; get; } = new JsonSerializer
        {
            TypeNameHandling = TypeNameHandling.All
        };
    }
}