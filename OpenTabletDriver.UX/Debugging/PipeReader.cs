using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.UX.Debugging
{
    public class PipeReader : IDisposable
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
                            if (Cast<DebugTabletReport>(jsonObject) is ITabletReport tabletReport) 
                                Report?.Invoke(this, tabletReport);
                            if (Cast<DebugAuxReport>(jsonObject) is IAuxReport auxReport)
                                Report?.Invoke(this, auxReport);
                        }
                    }
                }
                catch (Exception)
                {
                    // Ignore any exception here, just stop reading instead of throwing.
                }
            }
        }

        private T Cast<T>(JObject jsonObject) where T : class, IDeviceReport
        {
            return jsonObject.ToObject<T>() is T parsedReport && Validate(parsedReport) ? parsedReport : null;
        }

        private bool Validate(IDeviceReport report)
        {
            if (report is ITabletReport tabletReport)
                return tabletReport.Raw != null && tabletReport.PenButtons != null;
            else if (report is IAuxReport auxReport)
                return auxReport.Raw != null && auxReport.AuxButtons != null;
            else
                return true;
        }

        public void Dispose()
        {
            Reading = false;
            cancelSource.Cancel();
        }
    }
}