using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using OpenTabletDriver.Plugin;
using StreamJsonRpc;

namespace OpenTabletDriver.Desktop.RPC
{
    public class RpcHost<T> where T : new()
    {
        private JsonRpc rpc;
        private readonly string pipeName;

        public event EventHandler<bool> ConnectionStateChanged;
        public T Instance { protected set; get; }

        public RpcHost(T host, string pipeName)
        {
            this.pipeName = pipeName;
            this.Instance = host;
        }

        public RpcHost(string pipeName) : this(new T(), pipeName)
        {
        }

        public async Task Main()
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
                        this.rpc = JsonRpc.Attach(stream, Instance);
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
