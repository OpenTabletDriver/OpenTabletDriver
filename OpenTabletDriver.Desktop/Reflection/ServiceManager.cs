using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.DependencyInjection;

namespace OpenTabletDriver.Desktop.Reflection
{
    public class ServiceManager : IServiceManager
    {
        private readonly IDictionary<Type, Func<object>> services = new Dictionary<Type, Func<object>>();
        private readonly IDictionary<Type, object> initializedServices = new Dictionary<Type, object>();

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
            foreach (var service in initializedServices)
            {
                if (service.Value is not IDisposable disposable) continue;
                Log.Write("ResetServices", $"Disposing {disposable.GetType().Name}", LogLevel.Debug);
                disposable.Dispose();
            }
            initializedServices.Clear();
            services.Clear();
        }

        public object GetService(Type serviceType)
        {
            var rv = services.TryGetValue(serviceType, out Func<object> value) ? value.Invoke() : null;
            initializedServices.TryAdd(serviceType, rv);
            return rv;
        }

        public T GetService<T>() where T : class => GetService(typeof(T)) as T;
    }
}
