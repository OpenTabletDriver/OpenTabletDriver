using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using OpenTabletDriver.Configurations;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Plugin.Components;
using OpenTabletDriver.Plugin.Tablet;
using Xunit;

namespace OpenTabletDriver.Tests.Data
{
    public record TestTabletConfiguration
    {
        public required Lazy<TabletConfiguration> Configuration { get; init; }
        public required FileInfo File { get; init; }
        public required Lazy<string> FileContents { get; init; }

        private const string SKIP_TESTS_ATTRIBUTE_KEY = "SkipTests";

        public IEnumerable<TestTypes> SkippedTestTypes
        {
            get
            {
                if (this.Configuration.Value.Attributes?.ContainsKey(SKIP_TESTS_ATTRIBUTE_KEY) ?? false)
                {
                    foreach (string testType in this.Configuration.Value.Attributes[SKIP_TESTS_ATTRIBUTE_KEY]
                                 .Split(','))
                    {
                        if (!Enum.TryParse<TestTypes>(testType, out var result))
                            throw new ArgumentException($"Invalid value type {testType}");
                        yield return result;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Attribute values for the test disablement key. These should be serialized as a comma-separated string
    /// </summary>
    public enum TestTypes
    {
        LPI_DIGITIZER_X, // Lines per inches/mm test for X axis on digitizer
        LPI_DIGITIZER_Y, // Lines per inches/mm test for Y axis on digitizer
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

        private static readonly ServiceProvider ServiceProvider = new DriverServiceCollection().BuildServiceProvider();

        private static T GetRequiredService<T>() where T : notnull => ServiceProvider.GetRequiredService<T>();

        public static IReportParserProvider ReportParserProvider => GetRequiredService<IReportParserProvider>();
        public static IDeviceConfigurationProvider DeviceConfigurationProvider => GetRequiredService<IDeviceConfigurationProvider>();

        private static IEnumerable<string> parsersInConfigs => from configuration in DeviceConfigurationProvider.TabletConfigurations
                                                               from identifier in configuration.DigitizerIdentifiers.Concat(configuration.AuxiliaryDeviceIdentifiers ?? Enumerable.Empty<DeviceIdentifier>())
                                                               orderby identifier.ReportParser
                                                               select identifier.ReportParser;

        public static TheoryData<string> ParsersInConfigs => parsersInConfigs.Distinct().ToTheoryData();

        #region Schema

        private static JSchema? tabletConfigurationSchema;
        public static JSchema TabletConfigurationSchema => tabletConfigurationSchema ??= GetTabletConfigSchema();

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
