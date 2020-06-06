using System;
using System.IO;
using System.IO.Pipes;
using TabletDriverPlugin;

namespace OpenTabletDriverDaemon
{
    public class LogServer : IDisposable
    {
        public LogServer()
        {
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            PipeServer = new NamedPipeServerStream(Identifier.ToString());
            await PipeServer.WaitForConnectionAsync();
            StreamWriter = new StreamWriter(PipeServer);
            Log.Output += async (sender, message) =>
            {
                await StreamWriter.WriteLineAsync(Log.GetStringFormat(message));
                await StreamWriter.FlushAsync();
            };
        }

        public readonly Guid Identifier = Guid.NewGuid();
        private NamedPipeServerStream PipeServer { set; get; }
        private StreamWriter StreamWriter { set; get; }

        public void Dispose()
        {
            PipeServer.Disconnect();
            PipeServer.Dispose();
            PipeServer = null;
            StreamWriter = null;
        }
    }
}