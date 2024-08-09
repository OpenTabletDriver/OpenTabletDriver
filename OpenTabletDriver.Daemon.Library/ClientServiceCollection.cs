using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Daemon.Contracts;
using OpenTabletDriver.Daemon.Contracts.RPC;

namespace OpenTabletDriver.Daemon
{
    using static ServiceDescriptor;

    public class ClientServiceCollection : ServiceCollection
    {
        private static readonly IEnumerable<ServiceDescriptor> RequiredServices = new[]
        {
            Singleton(p => p.CreateInstance<RpcClient<IDriverDaemon>>("OpenTabletDriver.Daemon")),
            Transient(p => p.GetRequiredService<RpcClient<IDriverDaemon>>().Instance!)
        };

        protected ClientServiceCollection()
        {
            this.AddServices(RequiredServices);
        }

        protected ClientServiceCollection(IEnumerable<ServiceDescriptor> overridingServices) : this()
        {
            this.AddServices(overridingServices);
        }
    }
}
