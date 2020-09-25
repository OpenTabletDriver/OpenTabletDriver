using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using OpenTabletDriver.Plugin;
using StreamJsonRpc;

namespace OpenTabletDriver.RPC
{
    public class RpcHost<T> : IDisposable where T : new()
    {
        public RpcHost(string pipeName)
        {
            this.pipeName = pipeName;
            Reset();
            _ = Main();
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
                    this.rpc.Disconnected += (sender, e) => this.stream.Dispose();
                    await this.rpc.Completion;
                }
                catch (ObjectDisposedException)
                {
                    Reset();
                }
                catch (IOException)
                {
                    Reset();
                }
                catch (Exception ex)
                {
                    Log.Exception(ex);
                    Reset();
                }
            }
        }

        private void Reset()
        {
            IsConnected = false;
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
                PipeOptions.Asynchronous | PipeOptions.WriteThrough
            );
        }

        private readonly string pipeName;
        private JsonRpc rpc;
        private NamedPipeServerStream stream;
        private bool connected;

        public event EventHandler<bool> ConnectionStateChanged;

        public T Instance { protected set; get; } = new T();
        
        public bool IsConnected
        {
            protected set
            {
                this.connected = value;
                ConnectionStateChanged?.Invoke(this, value);
            }
            get => this.connected;
        }

        public void Dispose()
        {
            rpc.Dispose();
            stream?.Dispose();
        }
    }
}