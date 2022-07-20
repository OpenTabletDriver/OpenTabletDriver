using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

#nullable enable

namespace OpenTabletDriver.Desktop.Components
{
    [PublicAPI]
    public class SleepDetectionThread : IDisposable
    {
        private readonly Stopwatch _stopwatch = new();
        private readonly Action _action;
        private Task? _task;
        private CancellationTokenSource? _cancellationTokenSource;
        private double _prev;

        public SleepDetectionThread(Action action)
        {
            _action = action;
        }

        public void Start()
        {
            if (_task != null)
            {
                Stop();
            }

            _cancellationTokenSource = new();
            _task = DetectionLoop(_cancellationTokenSource.Token);
        }

        public void Stop()
        {
            if (_task != null)
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
                _task = null;
            }
        }

        private async Task DetectionLoop(CancellationToken cancellationToken)
        {
            _stopwatch.Start();
            while (!cancellationToken.IsCancellationRequested)
            {
                var elapsed = _stopwatch.Elapsed.TotalSeconds;
                if (elapsed - _prev > 2)
                    _action?.Invoke();

                _prev = elapsed;

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
            _stopwatch.Stop();
        }

        public void Dispose()
        {
            Stop();
            GC.SuppressFinalize(this);
        }
    }
}
