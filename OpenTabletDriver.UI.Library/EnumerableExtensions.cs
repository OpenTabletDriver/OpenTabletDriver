namespace OpenTabletDriver.UI;

#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks

public static class EnumerableExtensions
{
    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
    {
        foreach (var item in enumerable)
            action(item);
    }

    public static async Task ForEachAsync<T>(this Task<IEnumerable<T>> enumerable, Action<T> action)
    {
        foreach (var item in await enumerable)
            action(item);
    }

    public static async Task ForEachAsync<T>(this Task<IEnumerable<T>> enumerable, Func<T, Task> action)
    {
        foreach (var item in await enumerable)
            await action(item);
    }

    public static async Task ForEachAsync<T>(this IAsyncEnumerable<T> enumerable, Action<T> action)
    {
        await foreach (var item in enumerable)
            action(item);
    }

    public static async Task ForEachAsync<T>(this IAsyncEnumerable<T> enumerable, Func<T, Task> action)
    {
        await foreach (var item in enumerable)
            await action(item);
    }

    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
        foreach (var item in items)
            collection.Add(item);
    }
}
