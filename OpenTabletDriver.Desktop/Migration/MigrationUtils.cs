using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace OpenTabletDriver.Desktop.Migration
{
    internal static partial class MigrationUtils
    {
        public static TValue? SerializeMigrate<T, TValue>(this T obj) where T : IMigrate<TValue>
        {
            using var stringWriter = new StringWriter();
            using var textWriter = new JsonTextWriter(stringWriter);

            Serialization.Serializer.Serialize(textWriter, obj);

            using var stringReader = new StringReader(stringWriter.ToString());
            using var textReader = new JsonTextReader(stringReader);

            return Serialization.Serializer.Deserialize<TValue>(textReader);
        }

        public static IEnumerable<TValue> MigrateAll<T, TValue>(this IEnumerable<T?> enumerable, IServiceProvider serviceProvider) where T : IMigrate<TValue> where TValue : class
        {
            return enumerable.Select(v => v?.Migrate(serviceProvider))
                .Where(v => v != null)
                .Select(v => v!);
        }

        public static string MigrateNamespace(string path)
        {
            foreach (var (regex, replace) in NamespaceMigrationDict)
            {
                var match = regex.Match(path);
                if (match.Success)
                    path = string.Format(replace, match.Groups[1].Value);
            }

            return path;
        }

        private static readonly Dictionary<Regex, string> NamespaceMigrationDict = new Dictionary<Regex, string>
        {
            { TabletDriverLibRegex(), @"OpenTabletDriver.{0}" },
            { BindingRegex(), @"OpenTabletDriver.Desktop.Binding.{0}" },
            { OutputRegex(), @"OpenTabletDriver.Desktop.Output.{0}" },
            { PluginRegex(), @"OpenTabletDriver.{0}"}
        };

        [GeneratedRegex(@"TabletDriverLib\.(.+?)$")]
        private static partial Regex TabletDriverLibRegex();

        [GeneratedRegex(@"OpenTabletDriver\.Binding\.(.+?)$")]
        private static partial Regex BindingRegex();

        [GeneratedRegex(@"OpenTabletDriver\.Output\.(.+?)$")]
        private static partial Regex OutputRegex();

        [GeneratedRegex(@"OpenTabletDriver\.Plugin\.(.+?)$")]
        private static partial Regex PluginRegex();
    }
}
