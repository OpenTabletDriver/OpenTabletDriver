using System;
using System.IO;
using System.IO.Pipes;
using TabletDriverPlugin;

namespace OpenTabletDriver.Daemon
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
            StreamWriter = new StreamWriter(PipeServer)
            {
                AutoFlush = true
            };

            Log.Output += async (sender, message) =>
            {
                try
                {
                    if (PipeServer?.IsConnected ?? false)
                    StreamWriter.WriteLine(Log.GetStringFormat(message));
                }
                catch (IOException ioEx)
                {
                    Log.Exception(ioEx);
                    await PipeServer.WaitForConnectionAsync();
                }
            };
            Log.Debug($"Started log server {{{Identifier}}}");
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

            Log.Debug($"Stopped log server {{{Identifier}}}");
        }
    }
}