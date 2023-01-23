using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenTabletDriver.Daemon.Components
{
    public interface ISleepDetector
    {
        event Action Slept;
    }

    public class SleepDetector : ISleepDetector
    {
        private readonly object sync = new();
        private Task? task;
        private CancellationTokenSource? cts;
        private event Action? slept;

        public event Action Slept
        {
            add
            {
                if (slept == null)
                {
                    Start();
                }

                slept += value;
            }

            remove
            {
                slept -= value;

                if (slept == null)
                {
                    Stop();
                }
            }
        }

        private void Start()
        {
            lock (sync)
            {
                if (task != null)
                    return;

                cts = new();
                task = DetectionLoop(cts.Token);
            }
        }

        private void Stop()
        {
            lock (sync)
            {
                if (task == null)
                    return;

                cts!.Cancel();
#pragma warning disable VSTHRD002
                task.Wait();
#pragma warning restore VSTHRD002

                cts.Dispose();
                cts = null;
                task = null;
            }
        }

        private async Task DetectionLoop(CancellationToken ct)
        {
            var prev = DateTime.UtcNow;
            while (!ct.IsCancellationRequested)
            {
                var elapsed = DateTime.UtcNow;

                if (elapsed - prev > TimeSpan.FromSeconds(8))
                    slept?.Invoke();

                prev = elapsed;

                await Task.Delay(1000, ct);
            }
        }
    }
}
