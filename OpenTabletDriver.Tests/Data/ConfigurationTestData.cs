using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Plugin.Tablet;
using Xunit;

namespace OpenTabletDriver.Tests.Data
{
    public record TestTabletConfiguration
    {
        public required Lazy<TabletConfiguration> Configuration { get; init; }
        public required FileInfo File { get; init; }
        public required Lazy<string> FileContents  { get; init; }
    }

    public static class Extensions
    {
        public static TheoryData<T> ToTheoryData<T>(this IEnumerable<T> enumerable)
        {
            var result = new TheoryData<T>();
            foreach (var element in enumerable)
                result.Add(element);
            return result;
        }
    }

    public static partial class ConfigurationTestData
    {
        public static TheoryData<TestTabletConfiguration> TestTabletConfigurations =>
            GetTestTabletConfigurations();

        private static TheoryData<TestTabletConfiguration> GetTestTabletConfigurations()
        {
            var result = new TheoryData<TestTabletConfiguration>();
            foreach (var configFile in Directory.EnumerateFiles(GetConfigDir(), "*.json", SearchOption.AllDirectories))
            {
                FileInfo configFileInfo = new FileInfo(configFile);
                var ttc = new TestTabletConfiguration
                {
                    Configuration = new Lazy<TabletConfiguration>(() => Deserialize(configFileInfo)),
                    File = configFileInfo,
                    FileContents = new Lazy<string>(() => configFileInfo.OpenText().ReadToEnd())
                };
                result.Add(ttc);
            }

            return result;
        }

        #region Schema

        private static JSchema _tabletConfigurationSchema;
        public static JSchema TabletConfigurationSchema => _tabletConfigurationSchema ??= GetTabletConfigSchema();

        static JSchema GetTabletConfigSchema()
        {
            var gen = new JSchemaGenerator
            {
                DefaultRequired = Required.DisallowNull
            };

            var schema = gen.Generate(typeof(TabletConfiguration));
            DisallowAdditionalItemsAndProperties(schema);
            DisallowNullsAndEmptyCollections(schema);

            return schema;

            static void DisallowAdditionalItemsAndProperties(JSchema schema)
            {
                schema.AllowAdditionalItems = false;
                schema.AllowAdditionalProperties = false;
                schema.AllowUnevaluatedItems = false;
                schema.AllowUnevaluatedProperties = false;

                foreach (var child in schema.Properties)
                {
                    if (child.Key == nameof(TabletConfiguration.Attributes)) continue;
                    DisallowAdditionalItemsAndProperties(child.Value);
                }
            }

            static void DisallowNullsAndEmptyCollections(JSchema schema)
            {
                var schemaType = schema.Type!.Value;

                if (schemaType.HasFlag(JSchemaType.Array))
                {
                    schema.MinimumItems = 1;
                }
                else if (schemaType.HasFlag(JSchemaType.Object))
                {
                    schema.MinimumProperties = 1;
                }

                if (schema.Properties is not null)
                {
                    foreach (var property in schema.Properties)
                    {
                        DisallowNullsAndEmptyCollections(property.Value);
                    }
                }
            }
        }

        #endregion

        private static string GetConfigDir([CallerFilePath] string sourceFilePath = "") =>
            Path.GetFullPath(Path.Join(sourceFilePath, "../../../OpenTabletDriver.Configurations/Configurations"));

        private static TabletConfiguration Deserialize(FileInfo configFileInfo) => Serialization.Deserialize<TabletConfiguration>(configFileInfo);

        [GeneratedRegex(@"^OpenTabletDriver\.Tablet\..*$", RegexOptions.Compiled)]
        public static partial Regex AvaloniaReportParserPathRegex();
    }
}
