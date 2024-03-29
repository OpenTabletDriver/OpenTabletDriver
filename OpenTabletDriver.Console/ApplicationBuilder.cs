using System;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Daemon;

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
