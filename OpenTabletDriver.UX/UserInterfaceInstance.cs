using System;
using System.Threading;
using System.Threading.Tasks;
using OpenTabletDriver.Desktop.RPC;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.UX.RPC;

namespace OpenTabletDriver.UX
{
    public class UserInterfaceInstance : IDisposable
    {
        private readonly Instance instance = new Instance("OpenTabletDriver.UX.Instance");
        private readonly RpcHost<UserInterfaceProxy> host;
        private readonly RpcClient<IUserInterface> client;

        public UserInterfaceInstance()
        {
            if (!instance.AlreadyExists)
                host = new RpcHost<UserInterfaceProxy>("OpenTabletDriver.UX");
            else
                client = new RpcClient<IUserInterface>("OpenTabletDriver.UX");
        }

        public async Task Invoke(Func<IUserInterface, Task> action)
        {
            if (client != null)
            {
                await client.Connect();
                await action(client.Instance);
            }
            else
            {
                var thread = new Thread(async () => await host.Main())
                {
                    IsBackground = true
                };
                thread.Start();
            }
        }

        public void Dispose()
        {
            instance?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}