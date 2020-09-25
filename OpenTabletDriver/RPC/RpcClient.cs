using System;
using System.IO.Pipes;
using System.Threading.Tasks;
using StreamJsonRpc;

namespace OpenTabletDriver.RPC
{
    public class RpcClient<T> : IDisposable where T : class
    {
        public RpcClient(string pipeName)
        {
            this.pipeName = pipeName;
            Reset();
        }

        public async Task Connect()
        {
            await stream.ConnectAsync();
            IsConnected = true;
            Instance = JsonRpc.Attach<T>(stream);
        }

        private string pipeName;
        private NamedPipeClientStream stream;
        public T Instance { private set; get; }
        public bool IsConnected { private set; get; }

        private void Reset()
        {
            this.stream?.Dispose();
            this.stream = GetStream(this.pipeName);
        }

        private static NamedPipeClientStream GetStream(string name)
        {
            return new NamedPipeClientStream(
                ".",
                name,
                PipeDirection.InOut,
                PipeOptions.Asynchronous | PipeOptions.WriteThrough
            );
        }

        public void Dispose()
        {
            IsConnected = false;
            stream.Close();
            Instance = null;
        }
    }
}