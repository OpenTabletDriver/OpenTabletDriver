using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace OpenTabletDriver.Interop
{
    public class SleepDetectionThread : IDisposable
    {
        private readonly Stopwatch stopwatch = new();
        private readonly Action action;
        private Task task;
        private CancellationTokenSource cancellationTokenSource;
        private double prev;

        public SleepDetectionThread(Action action)
        {
            this.action = action;
        }

        public void Start()
        {
            if (task != null)
            {
                Stop();
            }

            cancellationTokenSource = new();
            task = DetectionLoop(cancellationTokenSource.Token);
        }

        public void Stop()
        {
            if (task != null)
            {
                cancellationTokenSource.Cancel();
                task.Wait();

                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
                task = null;
            }
        }

        private async Task DetectionLoop(CancellationToken cancellationToken)
        {
            stopwatch.Start();
            while (!cancellationToken.IsCancellationRequested)
            {
                var elapsed = stopwatch.Elapsed.TotalSeconds;
                if (elapsed - prev > 2)
                    action?.Invoke();

                prev = elapsed;

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
            stopwatch.Stop();
        }

        public void Dispose()
        {
            Stop();
            GC.SuppressFinalize(this);
        }
    }
}
