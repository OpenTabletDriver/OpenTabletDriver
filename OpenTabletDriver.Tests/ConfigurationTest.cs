using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using OpenTabletDriver.Components;
using OpenTabletDriver.Tablet;
using Xunit;
using Xunit.Abstractions;

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
            var serviceProvider = Utility.GetServices().BuildServiceProvider();
            var parserProvider = serviceProvider.GetRequiredService<IReportParserProvider>();
            var configurationProvider = serviceProvider.GetRequiredService<IDeviceConfigurationProvider>();

            var parsers = from configuration in configurationProvider.TabletConfigurations
                          from identifier in configuration.DigitizerIdentifiers.Concat(configuration.AuxiliaryDeviceIdentifiers)
                          orderby identifier.ReportParser
                          select identifier.ReportParser;

            var failed = false;

            foreach (var parserType in parsers.Distinct())
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

        [Fact]
        public void Configurations_DeviceIdentifier_IsNotConflicting()
        {
            var configurationProvider = Utility.GetServices()
                .BuildServiceProvider()
                .GetRequiredService<IDeviceConfigurationProvider>();

            var digitizerIdentificationContexts = from config in configurationProvider.TabletConfigurations
                                                  from identifier in config.DigitizerIdentifiers.Select((d, i) => new { DeviceIdentifier = d, Index = i })
                                                  select new IdentificationContext(config, identifier.DeviceIdentifier, IdentifierType.Digitizer, identifier.Index);

            var auxIdentificationContexts = from config in configurationProvider.TabletConfigurations
                                            from identifier in config.AuxiliaryDeviceIdentifiers.Select((d, i) => new { DeviceIdentifier = d, Index = i })
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

        private static readonly string ConfigurationProjectDir = Path.GetFullPath(Path.Join("../../../..", "OpenTabletDriver.Configurations"));
        private static readonly string ConfigurationDir = Path.Join(ConfigurationProjectDir, "Configurations");
        private static readonly IEnumerable<(string, string)> ConfigFiles = Directory.EnumerateFiles(ConfigurationDir, "*.json", SearchOption.AllDirectories)
            .Select(f => (Path.GetRelativePath(ConfigurationDir, f), File.ReadAllText(f)));

        [Fact]
        public void Configurations_Verify_Configs_With_Schema()
        {
            var gen = new JSchemaGenerator();
            var schema = gen.Generate(typeof(TabletConfiguration));
            DisallowAdditionalItemsAndProperties(schema);

            var failed = false;

            foreach (var (tabletFilename, tabletConfigString) in ConfigFiles)
            {
                var tabletConfig = JObject.Parse(tabletConfigString);
                if (tabletConfig.IsValid(schema, out IList<string> errors)) continue;

                _testOutputHelper.WriteLine($"Tablet Configuration {tabletFilename} did not match schema:\r\n{string.Join("\r\n", errors)}\r\n");
                failed = true;
            }

            Assert.False(failed);
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
            const int maxLinesToOutput = 3;

            var serializer = new JsonSerializer();
            var failedFiles = 0;

            var ourJsonSb = new StringBuilder();
            using var strw = new StringWriter(ourJsonSb);
            using var jtw = new JsonTextWriter(strw);
            jtw.Formatting = Formatting.Indented;
            jtw.Indentation = 2;

            foreach (var (tabletFilename, theirJson) in ConfigFiles)
            {
                ourJsonSb.Clear();
                var ourJsonObj = JsonConvert.DeserializeObject<TabletConfiguration>(theirJson);

                serializer.Serialize(jtw, ourJsonObj);
                ourJsonSb.AppendLine(); // otherwise we won't have an EOL at EOF

                var ourJson = ourJsonSb.ToString();

                var failedLines = DoesJsonMatch(ourJson, theirJson);

                if (failedLines.Any() || !string.Equals(theirJson, ourJson)) // second check ensures EOL markers are equivalent
                {
                    failedFiles++;
                    _testOutputHelper.WriteLine(
                        $"- Tablet Configuration '{tabletFilename}' lint check failed with the following errors:");

                    foreach (var (line, error) in failedLines.Take(maxLinesToOutput))
                        _testOutputHelper.WriteLine($"    Line {line}: {error}");
                    if (failedLines.Count > maxLinesToOutput)
                        _testOutputHelper.WriteLine($"    Truncated an additional {failedLines.Count - maxLinesToOutput} mismatching lines - wrong indent?");
                    else if (failedLines.Count == 0)
                        _testOutputHelper.WriteLine("     Generic mismatch (line endings?)");
                }
            }

            Assert.Equal(0, failedFiles);
        }

        private static IList<(int, string)> DoesJsonMatch(string ourJson, string theirJson)
        {
            int line = 0;
            var rv = new List<(int, string)>();

            using var ourSr = new StringReader(ourJson);
            using var theirSr = new StringReader(theirJson);
            while (true)
            {
                var ourLine = ourSr.ReadLine();
                var theirLine = theirSr.ReadLine();
                line++;

                if (ourLine == null && theirLine == null)
                    break; // success for file

                var ourLineOutput = ourLine ?? "EOF";
                var theirLineOutput = theirLine ?? "EOF";

                if (ourLine == null || theirLine == null || !string.Equals(ourLine, theirLine))
                    rv.Add((line, $"Expected '{ourLineOutput}' got '{theirLineOutput}'"));

                if (ourLine == null || theirLine == null)
                    break;
            }

            return rv;
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
            var pidMatch = a.VendorID == b.VendorID && a.ProductID == b.ProductID;
            var inputMatch = a.InputReportLength == b.InputReportLength || a.InputReportLength is null || b.InputReportLength is null;
            var outputMatch = a.OutputReportLength == b.OutputReportLength || a.OutputReportLength is null || b.OutputReportLength is null;

            if (pidMatch && inputMatch && outputMatch)
            {
                if (a.DeviceStrings.Count == 0 || b.DeviceStrings.Count == 0)
                    return true;

                var (longer, shorter) = a.DeviceStrings.Count > b.DeviceStrings.Count ? (a, b) : (b, a);
                return shorter.DeviceStrings.All(kv => longer.DeviceStrings.TryGetValue(kv.Key, out var otherValue) && otherValue == kv.Value);
            }

            return false;
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
