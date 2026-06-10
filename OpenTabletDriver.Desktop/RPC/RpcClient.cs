using System;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipes;
using System.Threading.Tasks;
using StreamJsonRpc;

namespace OpenTabletDriver.Desktop.RPC
{
    public class RpcClient<T> where T : class
    {
        private readonly string pipeName;
        private NamedPipeClientStream? stream;
        private JsonRpc? rpc;

        public T? Instance { private set; get; }

        [MemberNotNullWhen(true, nameof(Instance))]
        public bool IsConnected => Instance != null && rpc is { IsDisposed: false };

        public event EventHandler? Connected;
        public event EventHandler? Disconnected;

        public RpcClient(string pipeName)
        {
            this.pipeName = pipeName;
        }

        public async Task Connect()
        {
            this.stream = GetStream();
            await this.stream.ConnectAsync();

            rpc = new JsonRpc(this.stream);
            rpc.Disconnected += (_, _) =>
            {
                this.stream.Dispose();
                OnDisconnected();
                rpc.Dispose();
            };
            rpc.ExceptionStrategy = ExceptionProcessing.ISerializable;

            this.Instance = this.rpc.Attach<T>();
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
                this.pipeName,
                PipeDirection.InOut,
                PipeOptions.Asynchronous | PipeOptions.WriteThrough | PipeOptions.CurrentUserOnly
            );
        }
    }
}
