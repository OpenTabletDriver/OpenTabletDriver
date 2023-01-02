using System;

namespace OpenTabletDriver.Desktop.Migration
{
    internal interface IMigrate<out T>
    {
        T? Migrate(IServiceProvider serviceProvider);
    }
}
