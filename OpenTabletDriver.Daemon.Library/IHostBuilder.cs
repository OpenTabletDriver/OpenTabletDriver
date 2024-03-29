using System;
using Microsoft.Extensions.DependencyInjection;

namespace OpenTabletDriver.Daemon
{
    public interface IHostBuilder<out THost>
    {
        IHostBuilder<THost> ConfigureServices(Action<IServiceCollection> configure);

        /// <summary>
        /// Builds an instance of <see cref="THost"/>.
        /// </summary>
        /// <param name="serviceProvider">The final service provider associated to the <see cref="THost"/>.</param>
        /// <returns>The built host with its lifetime managed by <paramref name="serviceProvider"/>.</returns>
        THost Build(out IServiceProvider serviceProvider);
    }
}
