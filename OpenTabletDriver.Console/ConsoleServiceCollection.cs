using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Desktop;

namespace OpenTabletDriver.Console
{
    using static ServiceDescriptor;

    public class ConsoleServiceCollection : ClientServiceCollection
    {
        private static readonly IEnumerable<ServiceDescriptor> RequiredServices = new[]
        {
            Singleton<IApplicationLifetime, ApplicationLifetime>()
        };

        public ConsoleServiceCollection() : base(RequiredServices)
        {
        }
    }
}
