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
    }
}