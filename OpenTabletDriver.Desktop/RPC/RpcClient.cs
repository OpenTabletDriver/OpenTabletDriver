using System;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using StreamJsonRpc;

namespace OpenTabletDriver.Desktop.RPC
{
    /// <summary>
    /// Handles a single RPC client connection to <typeparamref name="T"/>.
    /// </summary>
    /// <param name="pipeName">The name of the <see cref="NamedPipeServerStream"/> to connect to.</param>
    /// <typeparam name="T">An RPC-compatible interface, e.g. <see cref="Contracts.IDriverDaemon"/>.</typeparam>
    public class RpcClient<T>(string pipeName)
        where T : class
    {
        public T? Instance { private set; get; }

        [MemberNotNullWhen(true, nameof(Instance))]
        public bool IsConnected => Instance != null;

        public event EventHandler? Connected;
        public event EventHandler? Disconnected;

        /// <exception cref="InvalidOperationException">
        /// If attempting to reconnect while an existing instance was active.
        /// Check <see cref="IsConnected"/> for <c>false</c> before invoking this.
        /// </exception>
        public async Task ConnectAsync(CancellationToken ct = default)
        {
            if (IsConnected)
                throw new InvalidOperationException("Cannot connect when already connected.");

            var stream = GetStream();
            await stream.ConnectAsync(ct).ConfigureAwait(false);

            var rpc = new JsonRpc(stream);

            rpc.Disconnected += (_, _) =>
            {
                Instance = null;
                rpc.Dispose();
                stream.Dispose();

                // emit disconnected event only when we're fully ready to handle a new connection
                OnDisconnected();
            };
            rpc.ExceptionStrategy = ExceptionProcessing.ISerializable;

            this.Instance = rpc.Attach<T>();
            rpc.StartListening();

            OnConnected();
        }

        protected virtual void OnConnected()
        {
            this.Connected?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnDisconnected()
        {
            this.Disconnected?.Invoke(this, EventArgs.Empty);
        }

        private NamedPipeClientStream GetStream()
        {
            return new NamedPipeClientStream(
                ".",
                pipeName,
                PipeDirection.InOut,
                PipeOptions.Asynchronous | PipeOptions.WriteThrough | PipeOptions.CurrentUserOnly
            );
        }
    }
}
