using System.Diagnostics;

namespace OpenTabletDriver.UI;

public static class TraceUtility
{
    private static int _counter = -1;

    [Conditional("DEBUG")]
    public static void PrintTrace(object source, string message)
    {
        var counter = Interlocked.Increment(ref _counter);
        Debug.WriteLine($"{counter}: [{source.GetType().Name} ({source.GetHashCode()})] {message}");
    }
}
