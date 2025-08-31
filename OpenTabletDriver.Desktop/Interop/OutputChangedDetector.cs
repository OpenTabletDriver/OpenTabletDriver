using System;
using System.Threading;
using System.Threading.Tasks;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin;

#nullable enable

namespace OpenTabletDriver.Desktop.Interop
{
    public interface IOutputChangedDetector
    {
        event Action Changed;
    }

    public class OutputChangedDetector() : TaskDetector(ResolutionDetector), IOutputChangedDetector
    {
        private const int LOOP_DURATION = 10;
        private static async Task ResolutionDetector(Action? action, CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(LOOP_DURATION), ct);

                if (DesktopInterop.HasDisplayLayoutChanged())
                {
                    Log.Write(nameof(OutputChangedDetector), "Display layout changed", LogLevel.Debug);
                    action?.Invoke();
                }
            }
        }

        public event Action Changed
        {
            add => ActionTask += value;
            remove => ActionTask -= value;
        }
    }
}
