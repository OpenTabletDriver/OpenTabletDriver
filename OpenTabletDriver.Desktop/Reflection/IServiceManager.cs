using System;

namespace OpenTabletDriver.Desktop.Reflection
{
    public interface IServiceManager : IServiceProvider
    {
        bool AddService<T>(Func<T> value) where T : class?;
        void ResetServices();
    }
}
