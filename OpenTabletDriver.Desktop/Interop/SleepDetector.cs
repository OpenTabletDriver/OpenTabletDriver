using System;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace OpenTabletDriver.Desktop.Interop
{
    public interface ISleepDetector
    {
        event Action Slept;
    }

    public class SleepDetector() : TaskDetector(DetectionLoop), ISleepDetector
    {
        public event Action Slept
        {
            add => ActionTask += value;
            remove => ActionTask -= value;
        }

        private static async Task DetectionLoop(Action? action, CancellationToken ct)
        {
            var prev = DateTime.UtcNow;
            while (!ct.IsCancellationRequested)
            {
                var elapsed = DateTime.UtcNow;

                if (elapsed - prev > TimeSpan.FromSeconds(8))
                    action?.Invoke();

                prev = elapsed;

                await Task.Delay(1000, ct);
            }
        }
    }
}
