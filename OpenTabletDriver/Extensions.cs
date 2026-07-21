using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver
{
    public static class Extensions
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

        extension<T>(IEnumerable<T?> o) where T : class
        {
            public IEnumerable<T> WhereNotNull() => o.Where(x => x != null).Cast<T>();
            public IEnumerable<TResult> SelectNotNull<TResult>(Func<T, TResult> fun) => o.WhereNotNull().Select(fun);
        }
    }
}
