using System;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.DependencyInjection;
using OpenTabletDriver.Plugin;

#nullable enable

namespace OpenTabletDriver
{
    public class DriverBuilder
    {
        private readonly DriverServiceCollection _driverServices;
        private bool _hasBuilt;

        public DriverBuilder()
        {
            _driverServices = new DriverServiceCollection();
        }

        public DriverBuilder(DriverServiceCollection serviceCollection)
        {
            _driverServices = serviceCollection;
        }

        public DriverBuilder ConfigureServices(Action<IServiceCollection> configure)
        {
            if (_hasBuilt)
                throw new DriverAlreadyBuiltException();

            configure(_driverServices);
            return this;
        }

        /// <summary>
        /// Builds an instance of <see cref="Driver"/>.
        /// </summary>
        /// <param name="serviceCollection">The final service collection associated to the driver.</param>
        /// <returns>The built Driver with its lifetime managed by <paramref name="serviceCollection"/>.</returns>
        public T Build<T>(out IServiceCollection serviceCollection) where T : class, IDriver
        {
            if (_hasBuilt)
                throw new DriverAlreadyBuiltException();

            _driverServices.AddSingleton<IDriver, T>();
#if DEBUG
            var serviceProvider = _driverServices.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateScopes = true,
                ValidateOnBuild = true
            });
#else
            var serviceProvider = _driverServices.BuildServiceProvider();
#endif
            _hasBuilt = true;
            serviceCollection = _driverServices;

            if (serviceProvider.GetService<IDriver>() is not T driver)
                throw new InvalidOperationException();

            return driver;
        }
    }

    public class DriverAlreadyBuiltException : Exception
    {
        public DriverAlreadyBuiltException() : this("A driver instance has already been built")
        {
        }

        public DriverAlreadyBuiltException(string? message) : base(message)
        {
        }

        public DriverAlreadyBuiltException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}