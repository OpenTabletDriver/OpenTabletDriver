using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace OpenTabletDriver.Desktop
{
    public static class DependencyInjectionExtensions
    {
        public static void AddServices(this IServiceCollection serviceCollection, IEnumerable<ServiceDescriptor> services)
        {
            var existingServiceTypes = serviceCollection.Select(s => s.ServiceType);
            var newServiceTypes = services.Select(s => s.ServiceType);
            var conflictingServiceTypes = existingServiceTypes.Intersect(newServiceTypes);

            var existingConflictingServices = serviceCollection.Where(
                s => conflictingServiceTypes.Contains(s.ServiceType)
            );

            serviceCollection.RemoveServices(existingConflictingServices.ToImmutableArray());

            foreach (var service in services)
            {
                serviceCollection.Add(service);
            }
        }

        public static void RemoveServices(this IServiceCollection serviceCollection, IEnumerable<ServiceDescriptor> services)
        {
            foreach (var service in services)
            {
                serviceCollection.Remove(service);
            }
        }

        public static object CreateInstance(this IServiceProvider serviceProvider, Type type, params object[] additionalDependencies)
        {
            var constructorQuery = from ctor in type.GetConstructors()
                let parameters = ctor.GetParameters()
                orderby parameters.Length
                select ctor;

            var constructor = constructorQuery.First();

            var argsQuery = from parameter in constructor.GetParameters()
                let paramType = parameter.ParameterType
                select additionalDependencies.FirstOrDefault(d => d.GetType().IsAssignableTo(paramType)) ?? serviceProvider.GetRequiredService(paramType);

            return constructor.Invoke(argsQuery.ToArray());
        }

        public static T CreateInstance<T>(this IServiceProvider serviceProvider, params object[] additionalDependencies)
        {
            return (T) serviceProvider.CreateInstance(typeof(T), additionalDependencies);
        }

        public static T GetOrCreateInstance<T>(this IServiceProvider serviceProvider, params object[] additionalDependencies)
        {
            if (serviceProvider.GetService<T>() is T service)
                return service;

            return serviceProvider.CreateInstance<T>(additionalDependencies);
        }
    }
}
