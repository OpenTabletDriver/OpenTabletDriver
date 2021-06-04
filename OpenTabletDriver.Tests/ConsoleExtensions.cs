using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace OpenTabletDriver.Tests
{
    public static class ConsoleExtensions
    {
        public static void WriteLines(this TextWriter tw, IEnumerable<string> strings) => tw.WriteLine(string.Concat(strings));
        public static void WriteLines(this TextWriter tw, params string[] strings) => WriteLines(tw, (IList<string>)strings);

        public static void WriteProperties<T>(this TextWriter tw, T source)
        {
            var properties = from property in typeof(T).GetProperties()
                select $"{property.Name}: {property.GetValue(source) ?? "{null}"}";

            tw.WriteLines(properties);
        }
    }
}