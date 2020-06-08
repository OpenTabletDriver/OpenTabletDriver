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
            Log.Output += (sender, message) =>
            {
                StreamWriter.WriteLine(Log.GetStringFormat(message));
                StreamWriter.Flush();
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