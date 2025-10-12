using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using OpenTabletDriver.Plugin.Components;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Tests.Data;
using Xunit;
using Xunit.Abstractions;

#nullable enable

namespace OpenTabletDriver.Tests
{
    public class ConfigurationTest(ITestOutputHelper testOutputHelper)
    {
        [Theory]
        [MemberData(nameof(ConfigurationTestData.ParsersInConfigs), MemberType = typeof(ConfigurationTestData))]
        public void Configurations_Have_ExistentParsers(string parserName)
        {
            ConfigurationTestData.ReportParserProvider.GetReportParser(parserName);
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

        [Theory]
        [MemberData(nameof(ConfigurationTestData.TestTabletConfigurations), MemberType = typeof(ConfigurationTestData))]
        public void Configurations_Verify_Configs_With_Schema(TestTabletConfiguration testTabletConfiguration)
        {
            var tabletFilename = testTabletConfiguration.File.Name;
            var tabletConfigString = testTabletConfiguration.FileContents.Value;
            var schema = ConfigurationTestData.TabletConfigurationSchema;
            IList<string> errors = new List<string>();

            var tabletConfig = JObject.Parse(tabletConfigString);
            try
            {
                Assert.True(tabletConfig.IsValid(schema, out errors));
            }
            catch (Exception)
            {
                if (errors.Any())
                    testOutputHelper.WriteLine($"Schema errors in {tabletFilename}: " + string.Join(",", errors));

                throw;
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
                                            from identifier in (config.AuxiliaryDeviceIdentifiers ?? Enumerable.Empty<DeviceIdentifier>()).Select((d, i) => new { DeviceIdentifier = d, Index = i })
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
        [Theory]
        [MemberData(nameof(ConfigurationTestData.TestTabletConfigurations), MemberType = typeof(ConfigurationTestData))]
        public void Configurations_Are_Linted(TestTabletConfiguration ttc)
        {
            var currentContent = ttc.FileContents.Value;

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

            var ourJsonObj = JsonConvert.DeserializeObject<TabletConfiguration>(currentContent);
            serializer.Serialize(jtw, ourJsonObj);
            sb.AppendLine();
            var expected = sb.ToString();

            var diff = InlineDiffBuilder.Diff(currentContent, expected, ignoreWhiteSpace: false);

            if (diff.HasDifferences)
            {
                testOutputHelper.WriteLine($"'{ttc.File.Name}' did not match linting:");
                PrintDiff(testOutputHelper, diff);
                Assert.True(false);
            }
        }

        private static readonly Regex AvaloniaReportParserPath = ConfigurationTestData.AvaloniaReportParserPathRegex();

        [Theory]
        [MemberData(nameof(ConfigurationTestData.TestTabletConfigurations), MemberType = typeof(ConfigurationTestData))]
        public void Configurations_Have_No_Legacy_Properties(TestTabletConfiguration ttc)
        {
            var errors = new List<string>();

            var config = ttc.Configuration.Value;
            var filePath = $"{ttc.File.Directory?.Name ?? "unknown"}/{ttc.File.Name}";

            // disable warning for "obsoleted" paths
#pragma warning disable CS0618 // Type or member is obsolete
            // aux identifier rename
            if (config.HasLegacyProperties())
                errors.Add("Incorrect key AuxilaryDeviceIdentifiers is present. It should be 'AuxiliaryDeviceIdentifiers'");

            // pen ButtonCount rename and type change
            if (config.Specifications.Pen.HasLegacyProperties())
                errors.Add("Incorrect key Specifications.Pen.Buttons is present. The Pen.Buttons.ButtonCount value should be moved to Pen.ButtonCount");
#pragma warning restore CS0618 // Type or member is obsolete

            // ReportParser path change is also indirectly tested elsewhere (as the class path won't exist on this version)
            // but it's still a good idea to test here, in case this test is run by itself
            if (config.DigitizerIdentifiers.Any(x => AvaloniaReportParserPath.IsMatch(x.ReportParser)))
                errors.Add("0.7-only ReportParser path detected. Replace all ReportPath instances of 'OpenTabletDriver.Tablet.' with 'OpenTabletDriver.Plugin.Tablet.'");

            string errorsFormatted = string.Join(Environment.NewLine, errors);
            Assert.True(errors.Count == 0, $"Errors detected in {filePath}:{Environment.NewLine}{errorsFormatted}");
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
