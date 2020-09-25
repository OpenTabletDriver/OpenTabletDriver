using System;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using OpenTabletDriver.Native;
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
            await Task.Delay(250);
            RpcInstance = new JsonRpc(stream);
            Instance = RpcInstance.Attach<T>();
            RpcInstance.StartListening();
        }

        private string pipeName;
        private NamedPipeClientStream stream;
        public JsonRpc RpcInstance { private set; get; }
        public T Instance { private set; get; }
        public bool IsConnected { private set; get; }

        private void Reset()
        {
            this.stream?.Dispose();
            this.stream = GetStream(this.pipeName);
        }

        private static NamedPipeClientStream GetStream(string name)
        {
            return new NamedPipeClientStream(".", name, PipeDirection.InOut, PipeOptions.Asynchronous);
        }

        public void Dispose()
        {
            IsConnected = false;
            stream.Close();
            Instance = null;
        }
    }
}