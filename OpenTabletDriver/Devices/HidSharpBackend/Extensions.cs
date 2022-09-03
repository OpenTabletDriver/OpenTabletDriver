using System;

namespace OpenTabletDriver.Devices.HidSharpBackend
{
    internal static class Extensions
    {
        private static bool TryGet<TSource, TValue>(
            this TSource source,
            Func<TSource, TValue> predicate,
            out TValue? value
        )
        {
            try
            {
                value = predicate(source);
                return true;
            }
            catch
            {
                value = default;
                return false;
            }
        }

        public static TValue SafeGet<TSource, TValue>(
            this TSource source,
            Func<TSource, TValue> predicate,
            TValue fallback
        ) => TryGet(source, predicate, out var value) ? value! : fallback;
    }
}
