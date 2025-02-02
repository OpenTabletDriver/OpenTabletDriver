using System;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Daemon;
using OpenTabletDriver.Daemon.Library;

namespace OpenTabletDriver.Console
{
    public class ApplicationBuilder : HostBuilder<IApplicationLifetime>
    {
        public ApplicationBuilder() : base(new ConsoleServiceCollection())
        {
        }

        public override IApplicationLifetime Build(out IServiceProvider serviceProvider)
        {
            serviceProvider = BuildServiceProvider();
            return serviceProvider.GetRequiredService<IApplicationLifetime>();
        }
    }
}
