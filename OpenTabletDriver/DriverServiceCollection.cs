using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenTabletDriver.ComponentProviders;
using OpenTabletDriver.Configurations;
using OpenTabletDriver.Devices;
using OpenTabletDriver.Plugin.Components;

namespace OpenTabletDriver
{
    public class DriverServiceCollection : ServiceCollection
    {
        private static IEnumerable<ServiceDescriptor> RequiredServices =>
        [
            ServiceDescriptor.Singleton<IReportParserProvider, ReportParserProvider>(),
            ServiceDescriptor.Singleton<IDeviceHubsProvider, DeviceHubsProvider>(serviceProvider => new DeviceHubsProvider(serviceProvider)),
            ServiceDescriptor.Singleton<ICompositeDeviceHub, RootHub>(RootHub.WithProvider),
            ServiceDescriptor.Singleton<IDeviceConfigurationProvider, DeviceConfigurationProvider>(),
        ];

        public DriverServiceCollection()
        {
            foreach (var serviceDescriptor in RequiredServices)
                this.Add(serviceDescriptor);
        }
    }
}
