using System;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace OpenTabletDriver.Desktop.Interop
{
    public class TaskDetector(Func<Action?, CancellationToken, Task> detector)
    {
        private readonly object _sync = new();
        private Task? _task;
        private CancellationTokenSource? _cts;

        private event Action? _ActionTask;

        protected event Action ActionTask
        {
            add
            {
                if (_ActionTask == null)
                {
                    Start(value);
                }

                _ActionTask += value;
            }

            remove
            {
                _ActionTask -= value;

                if (_ActionTask == null)
                {
                    Stop();
                }
            }
        }

        private void Start(Action action)
        {
            lock (_sync)
            {
                if (_task != null)
                    return;

                _cts = new CancellationTokenSource();
                _task = detector(action, _cts.Token);
            }
        }

        private void Stop()
        {
            lock (_sync)
            {
                if (_task == null)
                    return;

                _cts!.Cancel();
#pragma warning disable VSTHRD002
                _task.Wait();
#pragma warning restore VSTHRD002

                _cts.Dispose();
                _cts = null;
                _task = null;
            }
        }
    }
}
