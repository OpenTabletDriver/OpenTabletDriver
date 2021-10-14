using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using OpenTabletDriver.Plugin;
using StreamJsonRpc;

namespace OpenTabletDriver.Desktop.RPC
{
    public class RpcHost<T> where T : class
    {
        private JsonRpc rpc;
        private readonly string pipeName;

        public event EventHandler<bool> ConnectionStateChanged;

        public RpcHost(string pipeName)
        {
            this.pipeName = pipeName;
        }

        public async Task Run(T host)
        {
            while (true)
            {
                var stream = CreateStream();
                await stream.WaitForConnectionAsync();
                _ = Task.Run(async () =>
                {
                    try
                    {
                        ConnectionStateChanged?.Invoke(this, true);
                        this.rpc = JsonRpc.Attach(stream, host);
                        await this.rpc.Completion;
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                    catch (IOException)
                    {
                    }
                    catch (Exception ex)
                    {
                        Log.Exception(ex);
                    }
                    ConnectionStateChanged?.Invoke(this, false);
                    this.rpc.Dispose();
                    await stream.DisposeAsync();
                });
            }
        }

        private NamedPipeServerStream CreateStream()
        {
            return new NamedPipeServerStream(
                this.pipeName,
                PipeDirection.InOut,
                NamedPipeServerStream.MaxAllowedServerInstances,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous | PipeOptions.WriteThrough | PipeOptions.CurrentUserOnly
            );
        }
    }
}
