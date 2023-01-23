using System;
using Microsoft.Extensions.DependencyInjection;

namespace OpenTabletDriver.Daemon
{
    public abstract class HostBuilder<THost> : IHostBuilder<THost>
    {
        private readonly IServiceCollection _serviceCollection;

        protected HostBuilder(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }

        private bool _built;

        public virtual IHostBuilder<THost> ConfigureServices(Action<IServiceCollection> configure)
        {
            if (_built)
                throw new InvalidOperationException("Cannot configure services after the host has been built.");

            configure(_serviceCollection);
            return this;
        }

        protected IServiceProvider BuildServiceProvider()
        {
            if (_built)
                throw new InvalidOperationException("Cannot build service from the same service collection more than once.");

#if DEBUG
            var serviceProvider = _serviceCollection.BuildServiceProvider(new ServiceProviderOptions()
            {
                ValidateScopes = true,
                ValidateOnBuild = true
            });
#else
            var serviceProvider = _serviceCollection.BuildServiceProvider();
#endif
            _built = true;

            return serviceProvider;
        }

        public abstract THost Build(out IServiceProvider serviceProvider);

        public virtual THost Build() => Build(out _);
    }
}
