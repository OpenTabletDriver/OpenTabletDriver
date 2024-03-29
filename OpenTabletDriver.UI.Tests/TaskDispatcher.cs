using System;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace OpenTabletDriver.UI.Tests;

#pragma warning disable VSTHRD110

public class TaskDispatcher : IDispatcher
{
    public bool CheckAccess()
    {
        return true;
    }

    public void Post(Action action, DispatcherPriority priority = default)
    {
        Task.Run(action);
    }

    public void VerifyAccess()
    {
    }
}
