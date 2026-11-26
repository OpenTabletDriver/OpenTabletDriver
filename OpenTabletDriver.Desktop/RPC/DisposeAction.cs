using System;

namespace OpenTabletDriver.Desktop.RPC
{
    /// <summary>
    /// Wrapper class for running an action when disposed.
    /// </summary>
    /// <param name="disposeAction">The action to run on disposable.</param>
    public sealed class DisposeAction(Action disposeAction) : IDisposable
    {
        public void Dispose() => disposeAction();
    }
}
