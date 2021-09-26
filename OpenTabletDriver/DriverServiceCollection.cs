using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenTabletDriver.ComponentProviders;
using OpenTabletDriver.Configurations;
using OpenTabletDriver.Devices;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Devices;

#nullable enable

namespace OpenTabletDriver.DependencyInjection
{
    public class DriverServiceCollection : ServiceCollection
    {
        public static readonly ReadOnlyCollection<ServiceDescriptor> RequiredServices = new(new ServiceDescriptor[]
        {
            ServiceDescriptor.Singleton<IReportParserProvider, ReportParserProvider>(),
            ServiceDescriptor.Singleton<IDeviceHubsProvider, DeviceHubsProvider>(serviceProvider => new DeviceHubsProvider(serviceProvider)),
            ServiceDescriptor.Singleton<ICompositeDeviceHub, RootHub>(serviceProvider => RootHub.WithProvider(serviceProvider)),
            ServiceDescriptor.Singleton<IDeviceConfigurationProvider, DeviceConfigurationProvider>()
        });

        public DriverServiceCollection()
        {
            foreach (var serviceDescriptor in RequiredServices)
            {
                this.Add(serviceDescriptor);
            }
        }
    }
}