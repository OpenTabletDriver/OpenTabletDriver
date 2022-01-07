using System;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver
{
    internal static class Extensions
    {
        public static bool TryGet<TSource, TValue>(this TSource source, Func<TSource, TValue> predicate, out TValue value)
        {
            try
            {
                value = predicate(source);
                return true;
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
            value = default;
            return false;
        }

        public static TValue SafeGet<TSource, TValue>(this TSource source, Func<TSource, TValue> predicate, TValue fallback) => TryGet(source, predicate, out var value) ? value : fallback;
    }
}
