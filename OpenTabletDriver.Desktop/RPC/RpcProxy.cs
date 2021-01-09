using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenTabletDriver.Desktop.RPC
{
    public class RpcProxy<T> : IDisposable where T : class
    {
        private readonly string name;
        private readonly Instance instance;
        private RpcHost<T> host;
        private RpcClient<T> client;

        public RpcProxy(string name)
        {
            this.name = name;
            instance = new Instance($"{name}.Instance");
            if (instance.AlreadyExists)
                client = new RpcClient<T>(name);
        }

        public async Task InvokeAsync(Func<T, Task> action)
        {
            if (client != null)
            {
                await client.Connect();
                await action(client.Instance);
            }
        }

        public void Invoke(Func<T, Task> action)
        {
            var thread = new Thread(async () => await InvokeAsync(action));
            thread.Start();
            thread.Join();
        }

        public void Attach(T target)
        {
            if (client == null)
            {
                host = new RpcHost<T>(name);
                var thread = new Thread(async () => await host.Run(target))
                {
                    IsBackground = true
                };
                thread.Start();
            }
        }

        public void Dispose()
        {
            instance?.Dispose();
            host = null;
            client = null;
            GC.SuppressFinalize(this);
        }
    }
}