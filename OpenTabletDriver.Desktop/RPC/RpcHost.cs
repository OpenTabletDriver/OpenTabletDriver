using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using OpenTabletDriver.Plugin;
using StreamJsonRpc;

namespace OpenTabletDriver.Desktop.RPC
{
    public class RpcHost<T> where T : class
    {
        private readonly string pipeName;

        public event EventHandler<bool> ConnectionStateChanged;

        public RpcHost(string pipeName)
        {
            this.pipeName = pipeName;
        }

        public async Task Run(T host, CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                await Task.Run(async () =>
                {
                    await using var stream = CreateStream();
                    try
                    {
                        await stream.WaitForConnectionAsync(ct);
                    }
                    catch (OperationCanceledException)
                    {
                        // avoid throwing for intentionally canceled operations
                        if (ct.IsCancellationRequested)
                            return;

                        throw;
                    }

                    try
                    {
                        ConnectionStateChanged?.Invoke(this, true);
                        using var rpc = JsonRpc.Attach(stream, host);
                        await rpc.Completion.WaitAsync(ct);

                    }
                    catch (ObjectDisposedException)
                    {
                        // TODO: this empty catch clause can probably be removed now that
                        // the `stream` is self-contained within the delegate
                    }
                    catch (IOException)
                    {
                    }
                    catch (Exception ex)
                    {
                        Log.Exception(ex);
                    }

                    ConnectionStateChanged?.Invoke(this, false);
                }, CancellationToken.None);
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
