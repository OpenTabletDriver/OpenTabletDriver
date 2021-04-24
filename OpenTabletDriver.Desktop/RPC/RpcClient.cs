using System;
using System.IO.Pipes;
using System.Threading.Tasks;
using StreamJsonRpc;

namespace OpenTabletDriver.Desktop.RPC
{
    public class RpcClient<T> where T : class
    {
        private readonly string pipeName;
        private NamedPipeClientStream stream;
        private JsonRpc rpc;

        public T Instance { private set; get; }
        public event EventHandler Connected;
        public event EventHandler Disconnected;

        public RpcClient(string pipeName)
        {
            this.pipeName = pipeName;
        }

        public async Task Connect()
        {
            this.stream = GetStream();
            await this.stream.ConnectAsync();

            rpc = new JsonRpc(this.stream);
            this.Instance = this.rpc.Attach<T>();
            rpc.StartListening();

            rpc.Disconnected += (_, _) =>
            {
                this.stream.Dispose();
                Disconnected?.Invoke(this, null);
                rpc.Dispose();
            };
            Connected?.Invoke(this, null);
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
