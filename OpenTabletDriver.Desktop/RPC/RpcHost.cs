using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using StreamJsonRpc;

namespace OpenTabletDriver.Desktop.RPC
{
    public class RpcHost<T> where T : class
    {
        private JsonRpc _rpc;
        private readonly string _pipeName;

        public event EventHandler<bool> ConnectionStateChanged;

        public RpcHost(string pipeName)
        {
            _pipeName = pipeName;
        }

        public async Task Run(T host)
        {
            while (true)
            {
                var stream = CreateStream();
                await stream.WaitForConnectionAsync();
                _ = Task.Run(async () =>
                {
                    try
                    {
                        ConnectionStateChanged?.Invoke(this, true);
                        _rpc = Utilities.Host(stream, host);
                        _rpc.StartListening();
                        await _rpc.Completion;
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                    catch (IOException)
                    {
                    }
                    catch (Exception ex)
                    {
                        Log.Exception(ex);
                    }
                    ConnectionStateChanged?.Invoke(this, false);
                    _rpc.Dispose();
                    await stream.DisposeAsync();
                });
            }
        }

        private NamedPipeServerStream CreateStream()
        {
            return new NamedPipeServerStream(
                _pipeName,
                PipeDirection.InOut,
                NamedPipeServerStream.MaxAllowedServerInstances,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous | PipeOptions.WriteThrough | PipeOptions.CurrentUserOnly
            );
        }
    }
}
