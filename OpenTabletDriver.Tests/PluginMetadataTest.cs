using System;
using OpenTabletDriver.Desktop.Reflection.Metadata;
using Xunit;

namespace OpenTabletDriver.Tests
{
    public class PluginMetadataTest
    {
        public static TheoryData<Version, Version?, Version, bool> PluginMetadata_DeclaresDriverSupport_Properly_Data => new()
        {
            // Updated plugin
            { new Version("0.5.3.3"), null, new Version("0.5.3.3"), true },
            // Outdated plugin
            { new Version("0.5.3.3"), null, new Version("0.6.0.0"), false },
            // Slightly outdated plugin
            { new Version("0.5.2.0"), null, new Version("0.5.3.3"), true },
            // Slightly outdated driver
            { new Version("0.5.3.3"), null, new Version("0.5.2.0"), false },
            // Outdated driver
            { new Version("0.6.0.0"), null, new Version("0.5.3.3"), false },
            // Slightly outdated plugin with version limit
            { new Version("0.6.0.0"), new Version("0.6.5.1"), new Version("0.6.5.2"), false },
            // Slightly outdated driver with version limit
            { new Version("0.6.0.0"), new Version("0.6.5.1"), new Version("0.6.5.0"), true },
        };

        [Theory]
        [MemberData(nameof(PluginMetadata_DeclaresDriverSupport_Properly_Data))]
        public void PluginMetadata_DeclaresDriverSupport_Properly(Version supportedDriverVersion, Version? maxSupportedDriverVersion, Version driverVersion, bool expectedSupport)
        {
            var pluginMetaData = new PluginMetadata()
            {
                SupportedDriverVersion = supportedDriverVersion,
                MaxSupportedDriverVersion = maxSupportedDriverVersion
            };

            var supportStatus = pluginMetaData.IsSupportedBy(driverVersion);

            Assert.Equal(expectedSupport, supportStatus);
        }
    }
}
