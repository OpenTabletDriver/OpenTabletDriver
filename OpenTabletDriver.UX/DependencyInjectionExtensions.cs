using Microsoft.Extensions.DependencyInjection;

namespace OpenTabletDriver.UX
{
    public static class DependencyInjectionExtensions
    {
        public static ServiceDescriptor Singleton<T>() where T : class => ServiceDescriptor.Singleton<T, T>();
    }
}
