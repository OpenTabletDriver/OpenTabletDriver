using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Components;
using OpenTabletDriver.Plugin.Devices;

namespace OpenTabletDriver.ComponentProviders
{
    public class DeviceHubsProvider : IDeviceHubsProvider
    {
        public DeviceHubsProvider(IServiceProvider serviceProvider)
        {
            DeviceHubs = Assembly.GetExecutingAssembly().DefinedTypes
                .Where(type => type.IsAssignableTo(typeof(IDeviceHub))
                    && type.GetCustomAttribute<DeviceHubAttribute>() != null
                    && (type.GetCustomAttribute<SupportedPlatformAttribute>()?.IsCurrentPlatform ?? true))
                .Select(type => (IDeviceHub)ActivatorUtilities.CreateInstance(serviceProvider, type))
                .ToArray();
        }

        public IEnumerable<IDeviceHub> DeviceHubs { get; }
    }
}