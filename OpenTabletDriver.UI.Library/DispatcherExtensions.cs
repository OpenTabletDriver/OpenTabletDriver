using Avalonia.Threading;

namespace OpenTabletDriver.UI;

public static class DispatcherExtensions
{
    public static void ProbablySynchronousPost(this IDispatcher dispatcher, Action action, DispatcherPriority priority = default)
    {
        if (dispatcher.CheckAccess())
        {
            action();
        }
        else
        {
            dispatcher.Post(action, priority);
        }
    }

    public static void ScheduleOnce(this IDispatcher dispatcher, Action action, TimeSpan delay, DispatcherPriority priority = default, CancellationToken ct = default)
    {
        dispatcher.Post(async () =>
        {
            try
            {
                await Task.Delay(delay, ct);
            }
            catch (TaskCanceledException)
            {
                return;
            }

            action();
        }, priority);
    }

    public static void ScheduleOnce(this IDispatcher dispatcher, Action action, int ms, DispatcherPriority priority = default, CancellationToken ct = default)
    {
        dispatcher.ScheduleOnce(action, TimeSpan.FromMilliseconds(ms), priority, ct);
    }
}
