using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Devices;

namespace OpenTabletDriver.ComponentProviders
{
    public class DeviceHubsProvider : IDeviceHubsProvider
    {
        private readonly IDeviceHub[] _deviceHubs;

        public DeviceHubsProvider(IServiceProvider serviceProvider)
        {
            _deviceHubs = Assembly.GetExecutingAssembly().DefinedTypes
                .Where(type => type.IsAssignableTo(typeof(IDeviceHub))
                    && type.GetCustomAttribute<DeviceHubAttribute>() != null
                    && (type.GetCustomAttribute<SupportedPlatformAttribute>()?.IsCurrentPlatform ?? true))
                .Select(type => (IDeviceHub)ActivatorUtilities.CreateInstance(serviceProvider, type))
                .ToArray();
        }

        public IEnumerable<IDeviceHub> GetDeviceHubs()
        {
            return _deviceHubs;
        }
    }
}