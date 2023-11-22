using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
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
        private static readonly IEnumerable<(string,string)> ConfigFiles = Directory.EnumerateFiles(ConfigurationDir, "*.json", SearchOption.AllDirectories)
            .Select(f => (Path.GetRelativePath(ConfigurationDir, f), File.ReadAllText(f)));

        // JSONPath paths that do not need to be traversed but must still be presented as a key
        private static readonly string[] AllowedNullablePaths = {
            "$." + nameof(TabletConfiguration.AuxiliaryDeviceIdentifiers),
            "$." + nameof(TabletConfiguration.Specifications) + "." + nameof(TabletConfiguration.Specifications.Touch),
            "$." + nameof(TabletConfiguration.Specifications) + "." + nameof(TabletConfiguration.Specifications.AuxiliaryButtons),
            "$." + nameof(TabletConfiguration.Specifications) + "." + nameof(TabletConfiguration.Specifications.MouseButtons),
        };

        // Check all tablet configurations for JSON template deviations
        // Useful for ensuring that all interesting configuration fields have been filled out
        [Fact]
        public void Configurations_Matches_Template()
        {
            var jsonPaths = GetTabletConfigJsonPaths();
            var failed = false;

            foreach (var (tabletFilename, tabletConfigString) in ConfigFiles)
            {
                var obj = JObject.Parse(tabletConfigString);
                foreach (var path in jsonPaths.Where(path => obj.SelectToken(path) == null))
                {
                    _testOutputHelper.WriteLine($"Configuration file '{tabletFilename}' is missing JSON Path {path}");
                    failed = true;
                }
            }

            Assert.False(failed);
        }

        // Get all JSONPaths that a tablet configuration must have without exceptions
        private static IEnumerable<string> GetTabletConfigJsonPaths()
        {
            return GetAllJsonPathsFromInternalRecursive(typeof(TabletConfiguration), true)
                .Where(path =>
                {
                    foreach (var allowedPath in AllowedNullablePaths)
                    {
                        if (path.Contains(allowedPath))
                            // only retain specific allowedPath, paths nested below it are rejected
                            return path.Equals(allowedPath);
                    }

                    return true;
                });
        }

        // Recursively get all JSONPaths from supplied type
        private static IEnumerable<string> GetAllJsonPathsFromInternalRecursive(Type type, bool firstCall = true)
        {
            var prependString = firstCall ? "$." : "."; // valid JsonPath starts with $, e.g. '$.Path.To.Object'
            foreach (var member in type.GetProperties())
            {
                var isInternal = member.Module.Name.Contains("OpenTabletDriver");
                if (isInternal) // ensures we only emit our types
                    yield return prependString + member.Name;

                var memberType = member.PropertyType;
                var isArray = memberType.IsArray || (typeof(IEnumerable).IsAssignableFrom(memberType) && memberType != typeof(string));
                var memberName = member.Name + (isArray ? "[0]" : "");

                foreach (var innerMemberName in GetAllJsonPathsFromInternalRecursive(member.PropertyType, false))
                    yield return isInternal ?
                        prependString + memberName + innerMemberName :
                        innerMemberName;
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
