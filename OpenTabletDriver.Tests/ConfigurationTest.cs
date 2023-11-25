using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
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

        public static IEnumerable<object[]> Configurations_Have_ExistentParsers_Data
        {
            get
            {
                var configurationProvider = new DriverServiceCollection()
                    .BuildServiceProvider()
                    .GetRequiredService<IDeviceConfigurationProvider>();

                var parsers = configurationProvider.TabletConfigurations
                    .SelectMany(c => c.DigitizerIdentifiers)
                    .Concat(configurationProvider.TabletConfigurations
                        .SelectMany(c => c.AuxilaryDeviceIdentifiers))
                    .Select(i => i.ReportParser)
                    .Where(r => r != null)
                    .Distinct();

                foreach (var parser in parsers)
                    yield return new object[] { parser };
            }
        }

        [Theory]
        [MemberData(nameof(Configurations_Have_ExistentParsers_Data))]
        public void Configurations_Have_ExistentParsers(string reportParserName)
        {
            var parserProvider = new DriverServiceCollection()
                .BuildServiceProvider()
                .GetRequiredService<IReportParserProvider>();

            var reportParser = parserProvider.GetReportParser(reportParserName);

            Assert.NotNull(reportParser);
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

            var equality = IsEqual(identifier, identifier, new DeviceStringsComparer());

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

            var equality = IsEqual(identifier, otherIdentifier, new DeviceStringsComparer());

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

            var equality = IsEqual(identifier, otherIdentifier, new DeviceStringsComparer());

            Assert.True(equality);
        }

        [Fact]
        public void Configurations_DeviceIdentifier_Equality_DeviceStrings_SelfTest()
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
                DeviceStrings = new Dictionary<byte, string>
                {
                    [1] = "Test"
                },
                InputReportLength = 1,
                OutputReportLength = 1
            };

            var equality = IsEqual(identifier, otherIdentifier, new DeviceStringsComparer());

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
                    [1] = "Test"
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
                    [2] = "Test2"
                },
                InputReportLength = 1,
                OutputReportLength = 1
            };

            var equality = IsEqual(identifier, otherIdentifier, new DeviceStringsComparer());

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

        public static IEnumerable<object[]> Configurations_DeviceIdentifier_IsNotConflicting_Data
        {
            get
            {
                var configurationProvider = new DriverServiceCollection()
                    .BuildServiceProvider()
                    .GetRequiredService<IDeviceConfigurationProvider>();

                var digitizerIdentificationContexts = (from config in configurationProvider.TabletConfigurations
                                                       from identifier in config.DigitizerIdentifiers.Select((d, i) => new { DeviceIdentifier = d, Index = i })
                                                       select new IdentificationContext(config, identifier.DeviceIdentifier, IdentifierType.Digitizer, identifier.Index)).ToArray();

                var auxIdentificationContexts = (from config in configurationProvider.TabletConfigurations
                                                 from identifier in config.AuxilaryDeviceIdentifiers.Select((d, i) => new { DeviceIdentifier = d, Index = i })
                                                 select new IdentificationContext(config, identifier.DeviceIdentifier, IdentifierType.Auxilliary, identifier.Index)).ToArray();

                var identificationContexts = digitizerIdentificationContexts.Concat(auxIdentificationContexts);

                var encounteredPairs = new HashSet<IdentificationContextPair>();

                foreach (var identificationContext in identificationContexts)
                {
                    foreach (var otherIdentificationContext in identificationContexts.Where(c => !ReferenceEquals(identificationContext, c)))
                    {
                        // Yield return if unique pair
                        if (encounteredPairs.Add(new IdentificationContextPair(identificationContext, otherIdentificationContext)))
                            yield return new object[] { identificationContext, otherIdentificationContext };
                    }
                }
            }
        }

        [Theory]
        [MemberData(nameof(Configurations_DeviceIdentifier_IsNotConflicting_Data))]
        public void Configurations_DeviceIdentifier_IsNotConflicting(IdentificationContext deviceIdentifier, IdentificationContext otherIdentifier)
        {
            var comparer = new DeviceStringsComparer();

            var equality = IsEqual(deviceIdentifier.Identifier, otherIdentifier.Identifier, comparer);

            if (equality)
            {
                var message = string.Format("'{0}' {1} (index: {2}) conflicts with '{3}' {4} (index: {5})",
                    deviceIdentifier.TabletConfiguration.Name,
                    deviceIdentifier.IdentifierType,
                    deviceIdentifier.IdentifierIndex,
                    otherIdentifier.TabletConfiguration.Name,
                    otherIdentifier.IdentifierType,
                    otherIdentifier.IdentifierIndex);

                throw new Exception(message);
            }
        }

        private bool IsEqual(DeviceIdentifier a, DeviceIdentifier b, DeviceStringsComparer comparerInstance)
        {
            var pidMatch = a.VendorID == b.VendorID && a.ProductID == b.ProductID;
            var stringMatch = !a.DeviceStrings.Any() || !b.DeviceStrings.Any() || a.DeviceStrings.SequenceEqual(b.DeviceStrings, comparerInstance);
            var inputMatch = a.InputReportLength == b.InputReportLength || a.InputReportLength is null || b.InputReportLength is null;
            var outputMatch = a.OutputReportLength == b.OutputReportLength || a.OutputReportLength is null || b.OutputReportLength is null;

            return pidMatch && stringMatch && inputMatch && outputMatch;
        }

        public enum IdentifierType
        {
            Digitizer,
            Auxilliary
        }

        public record IdentificationContext(TabletConfiguration TabletConfiguration,
                                             DeviceIdentifier Identifier,
                                             IdentifierType IdentifierType,
                                             int IdentifierIndex);

        public struct IdentificationContextPair
        {
            public IdentificationContext A { get; }
            public IdentificationContext B { get; }

            public IdentificationContextPair(IdentificationContext a, IdentificationContext b)
            {
                // Order by name
                (A, B) = a.TabletConfiguration.Name.CompareTo(b.TabletConfiguration.Name) < 0 ? (a, b) : (b, a);
            }
        }

        public class DeviceStringsComparer : IEqualityComparer<KeyValuePair<byte, string>>
        {
            public bool Equals(KeyValuePair<byte, string> x, KeyValuePair<byte, string> y)
            {
                return x.Key == y.Key
                    && x.Value == y.Value;
            }

            public int GetHashCode([DisallowNull] KeyValuePair<byte, string> obj)
            {
                return HashCode.Combine(obj.Key, obj.Value);
            }
        }
    }
}
