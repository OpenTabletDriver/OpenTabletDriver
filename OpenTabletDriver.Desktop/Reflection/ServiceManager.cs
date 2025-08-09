using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OpenTabletDriver.Plugin.DependencyInjection;

namespace OpenTabletDriver.Desktop.Reflection
{
    public class ServiceManager : IServiceManager
    {
        private readonly IDictionary<Type, Func<object>> services = new Dictionary<Type, Func<object>>();

        /// <summary>
        /// Adds a retrieval method for a service type.
        /// </summary>
        /// <param name="value">The method in which returns the required service type.</param>
        /// <typeparam name="T">The type in which is returned by the constructor.</typeparam>
        /// <returns>True if adding the service was successful, otherwise false.</returns>
        public bool AddService<T>(Func<T> value)
        {
            return services.TryAdd(typeof(T), (value as Func<object>));
        }

        /// <summary>
        /// Clears all of the added services.
        /// </summary>
        public virtual void ResetServices()
        {
            services.Clear();
        }

        public object GetService(Type serviceType)
        {
            return services.ContainsKey(serviceType) ? services[serviceType].Invoke() : null;
        }

        public T GetService<T>() where T : class => GetService(typeof(T)) as T;
    }
}
