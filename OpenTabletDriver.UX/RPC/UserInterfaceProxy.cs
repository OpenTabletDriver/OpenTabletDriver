using System;
using System.Threading;
using System.Threading.Tasks;
using OpenTabletDriver.Desktop.RPC;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver.UX.RPC
{
    public class UserInterfaceProxy : IDisposable
    {
        private readonly Instance instance = new Instance("OpenTabletDriver.UX.Instance");
        private RpcHost<MainForm> host;
        private RpcClient<IUserInterface> client;

        public UserInterfaceProxy()
        {
            if (instance.AlreadyExists)
                client = new RpcClient<IUserInterface>("OpenTabletDriver.UX");
        }

        public async void Invoke(Func<IUserInterface, Task> action)
        {
            if (client != null)
            {
                await client.Connect();
                await action(client.Instance);
            }
        }

        public void Attach(MainForm form)
        {
            if (client == null)
            {
                host = new RpcHost<MainForm>("OpenTabletDriver.UX", form);
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
            host = null;
            client = null;
            GC.SuppressFinalize(this);
        }
    }
}