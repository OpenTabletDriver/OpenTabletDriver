using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace OpenTabletDriver.Tests
{
    public static class ConsoleExtensions
    {
        public static void WriteLines(this TextWriter tw, IList<string> strings) => tw.WriteLine(string.Concat(strings));
        public static void WriteLines(this TextWriter tw, params string[] strings) => WriteLines(tw, (IList<string>)strings);

        public static void WriteProperties<T>(this TextWriter tw, T source)
        {
            var properties = from property in typeof(T).GetProperties()
                select (Name: property.Name, Value: property.GetValue(source) ?? "{null}");

            foreach (var propertyPair in properties)
                tw.WriteLine($"{propertyPair.Name}: {propertyPair.Value}");
        }
    }
}