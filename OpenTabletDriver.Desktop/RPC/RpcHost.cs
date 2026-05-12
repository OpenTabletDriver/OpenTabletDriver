using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using OpenTabletDriver.Plugin;
using StreamJsonRpc;

namespace OpenTabletDriver.Desktop.RPC
{
    public class RpcHost<T>(string pipeName)
        where T : class
    {
        public event EventHandler<bool>? ConnectionStateChanged;

        public async Task Run(T host, CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                var stream = CreateStream();
                try
                {
                    await stream.WaitForConnectionAsync(ct);
                }
                catch (OperationCanceledException) { } // ignore exceptions caused by daemon shutting down

                _ = RespondToRpcRequestAsync(host, stream, ct);
            }
        }

        private async Task RespondToRpcRequestAsync(T host, NamedPipeServerStream stream, CancellationToken ct)
        {
            try
            {
                using var rpc = new JsonRpc(stream, stream, host);
                rpc.ExceptionStrategy = ExceptionProcessing.ISerializable;
                ConnectionStateChanged?.Invoke(this, true);
                rpc.StartListening();
                await rpc.Completion.WaitAsync(ct);
            }
            catch (TaskCanceledException) { } // ignore exceptions caused by daemon shutting down
            catch (Exception ex)
            {
                Log.Exception(ex);
            }

            ConnectionStateChanged?.Invoke(this, false);
            await stream.DisposeAsync();
        }

        private NamedPipeServerStream CreateStream()
        {
            return new NamedPipeServerStream(
                pipeName,
                PipeDirection.InOut,
                NamedPipeServerStream.MaxAllowedServerInstances,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous | PipeOptions.WriteThrough | PipeOptions.CurrentUserOnly
            );
        }
    }
}
