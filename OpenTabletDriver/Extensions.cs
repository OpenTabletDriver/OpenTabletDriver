using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver
{
    internal static class Extensions
    {
        public static bool TryGet<TSource, TValue>(this TSource source, Func<TSource, TValue> predicate, [NotNullWhen(true)] out TValue? value)
        {
            try
            {
                value = predicate(source);
                Debug.Assert(value != null);
                return true;
            }
            catch (Exception ex)
            {
                Log.Exception(ex, LogLevel.Debug);
            }
            value = default;
            return false;
        }

        public static TValue SafeGet<TSource, TValue>(this TSource source, Func<TSource, TValue> predicate, TValue fallback) =>
            source.TryGet(predicate, out var value) ? value : fallback;
    }
}
