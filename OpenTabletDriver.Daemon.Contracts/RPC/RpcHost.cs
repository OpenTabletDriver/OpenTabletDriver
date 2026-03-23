using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using OpenTabletDriver.Daemon.Contracts.RPC.Messages;
using StreamJsonRpc;

namespace OpenTabletDriver.Daemon.Contracts.RPC
{
    public class RpcHost<T> where T : class
    {
        private readonly SynchronizationContext _synchronizationContext;
        private readonly string _pipeName;
        private JsonRpc? _rpc;

        public RpcHost(SynchronizationContext synchronizationContext, string pipeName)
        {
            _synchronizationContext = synchronizationContext;
            _pipeName = pipeName;
        }

        public event EventHandler<bool>? ConnectionStateChanged;

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

                        _rpc = new JsonRpc(new MessageHandler(stream), host)
                        {
                            SynchronizationContext = _synchronizationContext
                        };

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
                    _rpc?.Dispose();
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
