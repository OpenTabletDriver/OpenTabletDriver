using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using StreamJsonRpc;

namespace OpenTabletDriver.RPC
{
    public class RpcHost<T> : IDisposable where T : new()
    {
        public RpcHost(string pipeName)
        {
            this.pipeName = pipeName;
            Reset();
            var mainTask = Main();
        }

        public async Task Main()
        {
            while (true)
            {
                try
                {
                    await this.stream.WaitForConnectionAsync();
                    IsConnected = true;
                    this.rpc = JsonRpc.Attach(this.stream, Instance);
                    await this.rpc.Completion;
                }
                catch (ObjectDisposedException)
                {
                    IsConnected = false;
                    Reset();
                }
            }
        }

        private void Reset()
        {
            this.stream?.Dispose();
            this.stream = CreateStream(this.pipeName);
        }
        
        private static NamedPipeServerStream CreateStream(string name)
        {
            return new NamedPipeServerStream(
                name,
                PipeDirection.InOut,
                1,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous
            );
        }

        private readonly string pipeName;
        private JsonRpc rpc;
        private NamedPipeServerStream stream;
        private bool isConnected;

        public event EventHandler<bool> ConnectionStateChanged;

        public T Instance { protected set; get; } = new T();
        
        public bool IsConnected
        {
            protected set
            {
                this.isConnected = value;
                ConnectionStateChanged?.Invoke(this, value);
            }
            get => this.isConnected;
        }

        public void Dispose()
        {
            rpc.Dispose();
            stream?.Dispose();
        }
    }
}