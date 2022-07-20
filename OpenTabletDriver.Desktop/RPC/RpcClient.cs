using System;
using System.IO.Pipes;
using System.Threading.Tasks;
using StreamJsonRpc;

#nullable enable

namespace OpenTabletDriver.Desktop.RPC
{
    public class RpcClient<T> where T : class
    {
        private readonly string _pipeName;

        public RpcClient(string pipeName)
        {
            _pipeName = pipeName;
        }

        private NamedPipeClientStream? _stream;
        private JsonRpc? _rpc;
        private readonly TimeSpan _connectTimeout = TimeSpan.FromSeconds(5);

        public T? Instance { private set; get; }

        public event EventHandler? Connected;
        public event EventHandler? Disconnected;

        public async Task Connect()
        {
            _stream = GetStream();
            var connect = _stream.ConnectAsync();
            var timeout = Task.Delay(_connectTimeout);
            var result = await Task.WhenAny(connect, timeout);
            if (result == timeout)
                throw new TimeoutException($"Connecting to daemon failed after {_connectTimeout.Seconds} seconds.");

            _rpc = Utilities.Client(_stream);
            _rpc.Disconnected += (_, _) => OnDisconnected();

            Instance = _rpc.Attach<T>();
            _rpc.StartListening();

            OnConnected();
        }

        public async Task Reconnect()
        {
            await _stream!.DisposeAsync();
            _rpc?.Dispose();
            await Task.Delay(500);
        }

        protected virtual void OnConnected()
        {
            Connected?.Invoke(this, EventArgs.Empty);
        }

        private void OnDisconnected()
        {
            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        private NamedPipeClientStream GetStream()
        {
            return new NamedPipeClientStream(
                ".",
                _pipeName,
                PipeDirection.InOut,
                PipeOptions.Asynchronous | PipeOptions.WriteThrough | PipeOptions.CurrentUserOnly
            );
        }
    }
}
