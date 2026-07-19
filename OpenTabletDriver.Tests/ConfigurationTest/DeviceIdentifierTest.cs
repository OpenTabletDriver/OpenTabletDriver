using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Plugin.Components;
using OpenTabletDriver.Plugin.Tablet;
using Xunit;

namespace OpenTabletDriver.Tests.ConfigurationTest
{
    public partial class DeviceIdentifierTest
    {
        [Fact]
        public void Configurations_DeviceIdentifier_IsNotConflicting()
        {
            var configurationProvider = new DriverServiceCollection()
                .BuildServiceProvider()
                .GetRequiredService<IDeviceConfigurationProvider>();

            var digitizerIdentificationContexts = from config in configurationProvider.TabletConfigurations
                                                  from identifier in config.DigitizerIdentifiers.Select((d, i) => new { DeviceIdentifier = d, Index = i })
                                                  select new DeviceIdentifierTest.IdentificationContext(config, identifier.DeviceIdentifier, DeviceIdentifierTest.IdentifierType.Digitizer, identifier.Index);

            var auxIdentificationContexts = from config in configurationProvider.TabletConfigurations
                                            from identifier in (config.AuxiliaryDeviceIdentifiers ?? Enumerable.Empty<DeviceIdentifier>()).Select((d, i) => new { DeviceIdentifier = d, Index = i })
                                            select new DeviceIdentifierTest.IdentificationContext(config, identifier.DeviceIdentifier, DeviceIdentifierTest.IdentifierType.Auxiliary, identifier.Index);

            var identificationContexts = digitizerIdentificationContexts.Concat(auxIdentificationContexts);

            // group similar identifiers
            var groups = new Dictionary<DeviceIdentifierTest.IdentificationContext, List<DeviceIdentifierTest.IdentificationContext>>(IdentificationContextComparer.Default);

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
                    group = [identificationContext];
                }
            }

            static void AssertGroup(List<DeviceIdentifierTest.IdentificationContext> identificationContexts, DeviceIdentifierTest.IdentificationContext identificationContext)
            {
                foreach (var otherIdentificationContext in identificationContexts)
                {
                    AssertInequal(identificationContext, otherIdentificationContext);
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

            if ((a.Attributes?.TryGetValue("Interface", out string? aInterface) ?? false) && (b.Attributes?.TryGetValue("Interface", out string? bInterface) ?? false))
            {
                if (aInterface != bInterface)
                    return false;
            }

            if ((a.Attributes?.TryGetValue("HidReports", out string? aHidReports) ?? false) && (b.Attributes?.TryGetValue("HidReports", out string? bHidReports) ?? false))
            {
                if (aHidReports != bHidReports)
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

            public int GetHashCode(IdentificationContext obj)
            {
                return HashCode.Combine(
                    obj.Identifier.VendorID,
                    obj.Identifier.ProductID,
                    obj.Identifier.InputReportLength);
            }
        }
    }
}
