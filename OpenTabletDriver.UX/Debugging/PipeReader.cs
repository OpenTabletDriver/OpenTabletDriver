using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TabletDriverPlugin.Tablet;

namespace OpenTabletDriver.UX.Debugging
{
    public class PipeReader<T> : IDisposable where T : IDeviceReport
    {
        public PipeReader(Guid serverId)
        {
            ServerID = serverId;
            WorkerThread = new Thread(Main)
            {
                Name = "PipeReader Worker Thread"
            };
            WorkerThread.Start();
        }

        public bool Reading { private set; get; } = true;
        public virtual event EventHandler<IDeviceReport> Report;
        public Guid ServerID { private set; get; }

        private CancellationTokenSource cancelSource = new CancellationTokenSource();

        private Thread WorkerThread;
        private static readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };

        private async void Main()
        {
            var serializer = new JsonSerializer();
            using (var pipeClient = new NamedPipeClientStream(ServerID.ToString()))
            using (var sr = new StreamReader(pipeClient))
            using (var reader = new JsonTextReader(sr))
            {
                await pipeClient.ConnectAsync();
                try
                {
                    while (Reading)
                    {
                        await reader.ReadAsync(cancelSource.Token);
                        if (reader.TokenType == JsonToken.StartObject)
                        {
                            JObject jsonObject = (JObject)serializer.Deserialize(reader);
                            if (jsonObject.ToObject<T>() is T report) 
                                Report?.Invoke(this, report);
                        }
                    }
                }
                catch (Exception)
                {
                    // Ignore any exception here, just stop reading instead of throwing.
                }
            }
        }

        public void Dispose()
        {
            Reading = false;
            cancelSource.Cancel();
        }
    }
}