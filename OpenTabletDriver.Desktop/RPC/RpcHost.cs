using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using OpenTabletDriver.Plugin;
using StreamJsonRpc;

namespace OpenTabletDriver.Desktop.RPC
{
    // Advice: Do not touch this class unless you've read https://microsoft.github.io/vs-streamjsonrpc/docs/getting-started.html
    public class RpcHost<T>(string pipeName)
        where T : class
    {
        public event EventHandler<bool>? ConnectionStateChanged;

        public async Task Run(T host, CancellationToken ct = default)
        {
            while (!ct.IsCancellationRequested)
            {
                // nothing happened yet, or we just handled a client, create new stream (other client's streams already saved their JsonRpc instance)
                // ...and not creating a new stream gives an InvalidOperationException with the message "Already in a connected state."
                var stream = CreateStream(); // disposal manually handled in responder, no 'using' is intentional here
                try
                {
                    await stream.WaitForConnectionAsync(ct);
                }
                catch (OperationCanceledException) { } // ignore exceptions caused by daemon shutting down

                _ = RespondToRpcRequestAsync(host, stream, ct);
            }
        }

        private async Task respondToRpcRequestAsync(T host, NamedPipeServerStream stream, CancellationToken ct = default)
        {
            try
            {
                using var rpc = new JsonRpc(stream, stream, host);

                // Increase exception fidelity
                rpc.ExceptionStrategy = ExceptionProcessing.ISerializable;

                ConnectionStateChanged?.Invoke(this, true);
                rpc.StartListening();

                // Wait for completion or cancellation token
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
