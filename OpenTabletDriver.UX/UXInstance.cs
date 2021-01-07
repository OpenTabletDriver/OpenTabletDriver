using System;
using System.Threading;
using System.Threading.Tasks;
using OpenTabletDriver.Desktop.RPC;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.UX.RPC;

namespace OpenTabletDriver.UX
{
    public class UXInstance : IDisposable
    {
        readonly Instance Instance = new Instance("OpenTabletDriver.UX.Instance");
        readonly RpcHost<UXDriver> Host;
        readonly RpcClient<IUXDriver> Client;
        readonly bool IsClient;

        public UXInstance()
        {
            if (!Instance.AlreadyExists)
            {
                Host = new RpcHost<UXDriver>("OpenTabletDriver.UX");
                IsClient = false;
            }
            else
            {
                Client = new RpcClient<IUXDriver>("OpenTabletDriver.UX");
                IsClient = true;
            }
        }

        public async Task Invoke(Func<IUXDriver, Task> action)
        {
            if (IsClient)
            {
                await Client.Connect();
                await action(Client.Instance);
            }
            else
            {
                var thread = new Thread(async () => await Host.Main())
                {
                    IsBackground = true
                };
                thread.Start();
            }
        }

        public void Dispose()
        {
            Instance?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}