using System;
using System.Collections.Generic;

namespace OpenTabletDriver.Desktop.Reflection
{
    public class ServiceManager : IServiceManager
    {
        private readonly Dictionary<Type, Func<object>> services = new();

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
        /// Clears all added services.
        /// </summary>
        public virtual void ResetServices()
        {
            services.Clear();
        }

        public object GetService(Type serviceType)
        {
            return services.TryGetValue(serviceType, out var value) ? value.Invoke() : null;
        }

        public T GetService<T>() where T : class => GetService(typeof(T)) as T;
    }
}
