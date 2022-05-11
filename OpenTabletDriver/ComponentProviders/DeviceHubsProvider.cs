using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Attributes;
using OpenTabletDriver.Components;
using OpenTabletDriver.Devices;

namespace OpenTabletDriver.ComponentProviders
{
    /// <summary>
    /// Reflection-sourced device hub provider.
    /// </summary>
    [PublicAPI]
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

            LegacyDeviceHubs = Assembly.GetExecutingAssembly().DefinedTypes
                .Where(type => type.IsAssignableTo(typeof(ILegacyDeviceHub))
                    && type.GetCustomAttribute<LegacyDeviceHubAttribute>() != null
                    && (type.GetCustomAttribute<SupportedPlatformAttribute>()?.IsCurrentPlatform ?? true))
                .Select(type => (ILegacyDeviceHub)ActivatorUtilities.CreateInstance(serviceProvider, type))
                .ToArray();
        }

        /// <summary>
        /// An enumeration of the reflected device hubs.
        /// </summary>
        public IEnumerable<IDeviceHub> DeviceHubs { get; }

        public IEnumerable<ILegacyDeviceHub> LegacyDeviceHubs { get; }
    }
}
