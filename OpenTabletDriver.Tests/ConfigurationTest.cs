using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using OpenTabletDriver.Plugin.Components;
using OpenTabletDriver.Plugin.Tablet;
using Xunit;
using Xunit.Abstractions;

#nullable enable

namespace OpenTabletDriver.Tests
{
    public class ConfigurationTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public ConfigurationTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Configurations_Have_ExistentParsers()
        {
            var serviceProvider = new DriverServiceCollection().BuildServiceProvider();
            var parserProvider = serviceProvider.GetRequiredService<IReportParserProvider>();
            var configurationProvider = serviceProvider.GetRequiredService<IDeviceConfigurationProvider>();

            var parsers = from configuration in configurationProvider.TabletConfigurations
                          from identifier in configuration.DigitizerIdentifiers.Concat(configuration.AuxilaryDeviceIdentifiers ?? Enumerable.Empty<DeviceIdentifier>())
                          orderby identifier.ReportParser
                          select identifier.ReportParser;

            var failed = false;

            foreach (var parserType in parsers.Where(p => p != null).Distinct())
            {
                try
                {
                    var parser = parserProvider.GetReportParser(parserType);
                    _testOutputHelper.WriteLine(parser.ToString());
                }
                catch
                {
                    _testOutputHelper.WriteLine($"Unable to find report parser '{parserType}'");
                    failed = true;
                }
            }

            Assert.False(failed);
        }

        [Fact]
        public void Configurations_DeviceIdentifier_Equality_SelfTest()
        {
            var identifier = new DeviceIdentifier
            {
                VendorID = 1,
                ProductID = 1,
                DeviceStrings = new Dictionary<byte, string>
                {
                    [1] = "Test"
                },
                InputReportLength = 1,
                OutputReportLength = 1
            };

            var equality = IsEqual(identifier, identifier);

            Assert.True(equality);
        }

        [Fact]
        public void Configurations_DeviceIdentifier_Equality_NullInput_SelfTest()
        {
            var identifier = new DeviceIdentifier
            {
                VendorID = 1,
                ProductID = 1,
                InputReportLength = 1,
                OutputReportLength = 1
            };
            var otherIdentifier = new DeviceIdentifier
            {
                VendorID = 1,
                ProductID = 1,
                InputReportLength = null,
                OutputReportLength = 1
            };

            var equality = IsEqual(identifier, otherIdentifier);

            Assert.True(equality);
        }

        [Fact]
        public void Configurations_DeviceIdentifier_Equality_NullOutput_SelfTest()
        {
            var identifier = new DeviceIdentifier
            {
                VendorID = 1,
                ProductID = 1,
                InputReportLength = 1,
                OutputReportLength = 1
            };
            var otherIdentifier = new DeviceIdentifier
            {
                VendorID = 1,
                ProductID = 1,
                InputReportLength = 1,
                OutputReportLength = null
            };

            var equality = IsEqual(identifier, otherIdentifier);

            Assert.True(equality);
        }

        [Fact]
        public void Configurations_DeviceIdentifier_Equality_DeviceStrings_SelfTest()
        {
            // both no device strings
            var identifier = new DeviceIdentifier
            {
                VendorID = 1,
                ProductID = 1,
                InputReportLength = 1,
                OutputReportLength = 1
            };
            var otherIdentifier = new DeviceIdentifier
            {
                VendorID = 1,
                ProductID = 1,
                InputReportLength = 1,
                OutputReportLength = 1
            };

            var equality = IsEqual(identifier, otherIdentifier);

            Assert.True(equality);

            // both have the same device strings
            identifier = new DeviceIdentifier
            {
                VendorID = 1,
                ProductID = 1,
                DeviceStrings = new Dictionary<byte, string>
                {
                    [1] = "Test"
                },
                InputReportLength = 1,
                OutputReportLength = 1
            };
            otherIdentifier = new DeviceIdentifier
            {
                VendorID = 1,
                ProductID = 1,
                DeviceStrings = new Dictionary<byte, string>
                {
                    [1] = "Test"
                },
                InputReportLength = 1,
                OutputReportLength = 1
            };

            equality = IsEqual(identifier, otherIdentifier);

            Assert.True(equality);

            // one of them has no device strings
            identifier = new DeviceIdentifier
            {
                VendorID = 1,
                ProductID = 1,
                DeviceStrings = new Dictionary<byte, string>
                {
                },
                InputReportLength = 1,
                OutputReportLength = 1
            };
            otherIdentifier = new DeviceIdentifier
            {
                VendorID = 1,
                ProductID = 1,
                DeviceStrings = new Dictionary<byte, string>
                {
                    [1] = "Test"
                },
                InputReportLength = 1,
                OutputReportLength = 1
            };

            equality = IsEqual(identifier, otherIdentifier);

            Assert.True(equality);
        }

        [Fact]
        public void Configurations_DeviceIdentifier_NonEquality_DeviceStrings_SelfTest()
        {
            var identifier = new DeviceIdentifier
            {
                VendorID = 1,
                ProductID = 1,
                DeviceStrings = new Dictionary<byte, string>
                {
                    [1] = "Test1"
                },
                InputReportLength = 1,
                OutputReportLength = 1
            };
            var otherIdentifier = new DeviceIdentifier
            {
                VendorID = 1,
                ProductID = 1,
                DeviceStrings = new Dictionary<byte, string>
                {
                    [1] = "Test",
                },
                InputReportLength = 1,
                OutputReportLength = 1
            };

            var equality = IsEqual(identifier, otherIdentifier);

            Assert.False(equality);
        }

        private static readonly string ConfigurationProjectDir = Path.GetFullPath(Path.Join("../../../..", "OpenTabletDriver.Configurations"));
        private static readonly string ConfigurationDir = Path.Join(ConfigurationProjectDir, "Configurations");
        private static readonly IEnumerable<(string, string)> ConfigFiles = Directory.EnumerateFiles(ConfigurationDir, "*.json", SearchOption.AllDirectories)
            .Select(f => (Path.GetRelativePath(ConfigurationDir, f), File.ReadAllText(f)));

        [Fact]
        public void Configurations_Verify_Configs_With_Schema()
        {
            var gen = new JSchemaGenerator();
            gen.DefaultRequired = Required.DisallowNull;

            var schema = gen.Generate(typeof(TabletConfiguration));
            DisallowAdditionalItemsAndProperties(schema);
            DisallowNullsAndEmptyCollections(schema);

            var failed = false;

            foreach (var (tabletFilename, tabletConfigString) in ConfigFiles)
            {
                var tabletConfig = JObject.Parse(tabletConfigString);
                if (tabletConfig.IsValid(schema, out IList<string> errors)) continue;

                _testOutputHelper.WriteLine($"Tablet Configuration {tabletFilename} did not match schema:");
                foreach (var error in errors)
                    _testOutputHelper.WriteLine(error);
                _testOutputHelper.WriteLine(string.Empty);

                failed = true;
            }

            Assert.False(failed);
        }

        private static void DisallowAdditionalItemsAndProperties(JSchema schema)
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

        private static void DisallowNullsAndEmptyCollections(JSchema schema)
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

        [Fact]
        public void Configurations_DeviceIdentifier_IsNotConflicting()
        {
            var configurationProvider = new DriverServiceCollection()
                .BuildServiceProvider()
                .GetRequiredService<IDeviceConfigurationProvider>();

            var digitizerIdentificationContexts = from config in configurationProvider.TabletConfigurations
                                                  from identifier in config.DigitizerIdentifiers.Select((d, i) => new { DeviceIdentifier = d, Index = i })
                                                  select new IdentificationContext(config, identifier.DeviceIdentifier, IdentifierType.Digitizer, identifier.Index);

            var auxIdentificationContexts = from config in configurationProvider.TabletConfigurations
                                            from identifier in (config.AuxilaryDeviceIdentifiers ?? Enumerable.Empty<DeviceIdentifier>()).Select((d, i) => new { DeviceIdentifier = d, Index = i })
                                            select new IdentificationContext(config, identifier.DeviceIdentifier, IdentifierType.Auxiliary, identifier.Index);

            var identificationContexts = digitizerIdentificationContexts.Concat(auxIdentificationContexts);

            // group similar identifiers
            var groups = new Dictionary<IdentificationContext, List<IdentificationContext>>(IdentificationContextComparer.Default);

            foreach (var identificationContext in identificationContexts)
            {
                ref var group = ref CollectionsMarshal.GetValueRefOrAddDefault(groups, identificationContext, out var exists);
                if (group is not null)
                {
                    AssertGroup(group, identificationContext);
                    group.Add(identificationContext);
                }
                else
                {
                    group = new List<IdentificationContext> { identificationContext };
                }
            }

            static void AssertGroup(List<IdentificationContext> identificationContexts, IdentificationContext identificationContext)
            {
                foreach (var otherIdentificationContext in identificationContexts)
                {
                    AssertInequal(identificationContext, otherIdentificationContext);
                }
            }
        }

        /// <summary>
        /// Ensures that configuration formatting/linting matches expectations, which are:
        /// - 2 space indentation
        /// - Newline at end of file
        /// - Consistent newline format
        /// </summary>
        [Fact]
        public void Configurations_Are_Linted()
        {
            var serializer = new JsonSerializer()
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            var sb = new StringBuilder();
            using var strw = new StringWriter(sb);
            using var jtw = new JsonTextWriter(strw)
            {
                Formatting = Formatting.Indented,
                Indentation = 2
            };

            var failedFiles = 0;
            foreach (var (tabletFilename, actual) in ConfigFiles)
            {
                sb.Clear();
                try
                {
                    var ourJsonObj = JsonConvert.DeserializeObject<TabletConfiguration>(actual);
                    serializer.Serialize(jtw, ourJsonObj);
                    sb.AppendLine();
                    var expected = sb.ToString();

                    var diff = InlineDiffBuilder.Diff(expected, actual);

                    if (diff.HasDifferences)
                    {
                        _testOutputHelper.WriteLine($"'{tabletFilename}' did not match linting:");
                        PrintDiff(_testOutputHelper, diff);
                        failedFiles++;
                    }
                }
                catch (Exception ex)
                {
                    _testOutputHelper.WriteLine($"'{tabletFilename}' failed to deserialize: {ex.Message}");
                    failedFiles++;
                }
            }

            Assert.True(failedFiles == 0, $"{failedFiles} configuration files failed linting.");
        }

        private static void PrintDiff(ITestOutputHelper outputHelper, DiffPaneModel diff)
        {
            foreach (var line in diff.Lines)
            {
                switch (line.Type)
                {
                    case ChangeType.Inserted:
                        outputHelper.WriteLine($"+ {line.Text}");
                        break;
                    case ChangeType.Deleted:
                        outputHelper.WriteLine($"- {line.Text}");
                        break;
                    default:
                        outputHelper.WriteLine($"  {line.Text}");
                        break;
                }
            }
        }

        private static void AssertInequal(IdentificationContext a, IdentificationContext b)
        {
            if (IsEqual(a.Identifier, b.Identifier))
            {
                var message = string.Format("'{0}' {1} (index: {2}) conflicts with '{3}' {4} (index: {5})",
                    a.TabletConfiguration.Name,
                    a.IdentifierType,
                    a.IdentifierIndex,
                    b.TabletConfiguration.Name,
                    b.IdentifierType,
                    b.IdentifierIndex);

                throw new Exception(message);
            }
        }

        private static bool IsEqual(DeviceIdentifier a, DeviceIdentifier b)
        {
            if (a.VendorID != b.VendorID || a.ProductID != b.ProductID)
            {
                return false;
            }

            if (a.InputReportLength != b.InputReportLength && a.InputReportLength is not null && b.InputReportLength is not null)
            {
                return false;
            }

            if (a.OutputReportLength != b.OutputReportLength && a.OutputReportLength is not null && b.OutputReportLength is not null)
            {
                return false;
            }

            if (a.DeviceStrings is null || a.DeviceStrings.Count == 0 || b.DeviceStrings is null || b.DeviceStrings.Count == 0)
            {
                return true; // One or both have no device strings, so they match.
            }

            // Both have device strings, so check for equality.
            if (a.DeviceStrings.Count != b.DeviceStrings.Count)
            {
                return false;
            }

            return a.DeviceStrings.All(kv => b.DeviceStrings.TryGetValue(kv.Key, out var otherValue) && otherValue == kv.Value);
        }

        public enum IdentifierType
        {
            Digitizer,
            Auxiliary
        }

        public record IdentificationContext(
            TabletConfiguration TabletConfiguration,
            DeviceIdentifier Identifier,
            IdentifierType IdentifierType,
            int IdentifierIndex
        );

        private class IdentificationContextComparer : IEqualityComparer<IdentificationContext>
        {
            public static readonly IdentificationContextComparer Default = new IdentificationContextComparer();

            public bool Equals(IdentificationContext? x, IdentificationContext? y)
            {
                if (x is null && y is null)
                    return true;
                if (x is null || y is null)
                    return false;

                return IsEqual(x.Identifier, y.Identifier);
            }

            public int GetHashCode([DisallowNull] IdentificationContext obj)
            {
                return HashCode.Combine(
                    obj.Identifier.VendorID,
                    obj.Identifier.ProductID,
                    obj.Identifier.InputReportLength);
            }
        }
    }
}
