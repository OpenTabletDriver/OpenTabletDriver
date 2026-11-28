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
                var stream = CreateStream(); // disposal manually handled in responder, no 'using' is intentional here
                try
                {
                    await stream.WaitForConnectionAsync(ct).ConfigureAwait(false);
                    _ = respondToRpcRequestAsync(host, stream, ct);
                    stream = null; // successfully passed to responder, stream disposal is no longer our responsibility
                }
                catch (OperationCanceledException) { } // ignore exceptions caused by daemon shutting down
                finally
                {
                    if (stream != null) // stream unhandled, we must dispose
                        await stream.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        private async Task respondToRpcRequestAsync(T host, NamedPipeServerStream clientStream, CancellationToken ct = default)
        {
            await using var stream = clientStream; // ensure disposal

            try
            {
                using var rpc = new JsonRpc(stream, stream, host);

                // Increase exception fidelity
                rpc.ExceptionStrategy = ExceptionProcessing.ISerializable;

                ConnectionStateChanged?.Invoke(this, true);
                rpc.StartListening();

                // Wait for completion or cancellation token
                await rpc.Completion.WaitAsync(ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException) { } // ignore exceptions caused by daemon shutting down
            catch (Exception ex)
            {
                Log.Exception(ex);
            }

            ConnectionStateChanged?.Invoke(this, false);
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
