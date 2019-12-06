using System;
using System.Collections.Generic;
using System.Linq;

namespace TabletDriverLib
{
    internal static class Tools
    {
        public static IEnumerable<T> GetEnumValues<T>(Type type)
        {
            return Enum.GetValues(type).Cast<T>();
        }

        public static bool EnumContains<T>(this Type type, T obj)
        {
            var values = GetEnumValues<T>(type);
            return values.Contains(obj);
        }
    }
}