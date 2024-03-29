using System;
using OpenTabletDriver.Daemon.Contracts;
using Xunit;

namespace OpenTabletDriver.Tests
{
    public class PluginMetadataTest
    {
        public static TheoryData<Version, Version, bool> PluginMetadata_DeclaresDriverSupport_Properly_Data => new()
        {
            // Updated plugin
            { new Version("0.5.3.3"), new Version("0.5.3.3"), true },
            // Outdated plugin
            { new Version("0.5.3.3"), new Version("0.6.0.0"), false },
            // Slightly outdated plugin
            { new Version("0.5.2.0"), new Version("0.5.3.3"), true },
            // Slightly outdated driver
            { new Version("0.5.3.3"), new Version("0.5.2.0"), false },
            // Outdated driver
            { new Version("0.6.0.0"), new Version("0.5.3.3"), false }
        };

        [Theory]
        [MemberData(nameof(PluginMetadata_DeclaresDriverSupport_Properly_Data))]
        public void PluginMetadata_DeclaresDriverSupport_Properly(Version supportedDriverVersion, Version driverVersion, bool expectedSupport)
        {
            var pluginMetaData = new PluginMetadata()
            {
                SupportedDriverVersion = supportedDriverVersion
            };

            var supportStatus = pluginMetaData.IsSupportedBy(driverVersion);

            Assert.Equal(expectedSupport, supportStatus);
        }
    }
}
