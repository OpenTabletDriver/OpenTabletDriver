using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TabletDriverLib.Contracts;
using TabletDriverPlugin;
using TabletDriverPlugin.Tablet;

namespace OpenTabletDriver.UX.Debugging
{
    public class PipeReader<T> : IDeviceReader<T>, IDisposable where T : IDeviceReport
    {
        public PipeReader(string pipeName)
        {
            InitializeAsync(pipeName);
        }

        private async void InitializeAsync(string pipeName)
        {
            PipeClient = new NamedPipeClientStream(pipeName);
            await PipeClient.ConnectAsync();
            WorkerThread = new Thread(Main)
            {
                Name = "PipeReader Worker Thread"
            };
            WorkerThread.Start();
        }

        public NamedPipeClientStream PipeClient { private set; get; }
        public bool Reading { protected set; get; }
        public IReportParser<T> Parser { set; get; }
        public virtual event EventHandler<T> Report;

        private CancellationTokenSource cancelSource = new CancellationTokenSource();

        private Thread WorkerThread;
        private static readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };

        private async void Main()
        {
            Reading = true;
            var serializer = new JsonSerializer();
            using (var sr = new StreamReader(PipeClient))
            using (var reader = new JsonTextReader(sr))
            {
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