using System;
using System.Collections.Generic;
using System.Linq;
using OpenTabletDriver.Plugin.Components;
using OpenTabletDriver.Plugin.Devices;
using OpenTabletDriver.Tests.ConfigurationTest;
using Xunit;

namespace OpenTabletDriver.Tests.TabletMatching
{
    public class IdentifierTests
    {
        public IdentifierTests()
        {
            Plugin.Log.Output += (_, message) => { Console.WriteLine(message); };
            Plugin.Log.Write(nameof(IdentifierTests), "Initialized");
        }

        private static IDeviceConfigurationProvider DeviceConfigurationProvider { get; } =
            TestData.DeviceConfigurationProvider;

        [Theory]
        [MemberData(nameof(TabletMatching.SingleTabletIdentifiers), MemberType = typeof(TabletMatching))]
        public void SingleTabletIdentifiers_Only_Single_Config_Match(SingleTabletIdentifier tabletIdentifier)
        {
            var foundNames = new List<string>();

            var endpoints =
                tabletIdentifier.DeviceIdentifiers.Select(IDeviceEndpoint (x) =>
                    new MockedDeviceEndpoint(x, tabletIdentifier.DeviceStrings)).ToArray();

            foreach (var tabletConfig in DeviceConfigurationProvider.TabletConfigurations)
            {
                // aux untested, unsure if that's a problem
                var identifiers =
                    Driver.MatchDevice(tabletConfig, tabletConfig.DigitizerIdentifiers, endpoints, null);

                if (identifiers.Any())
                    foundNames.Add(tabletConfig.Name);
            }

            Assert.Single(foundNames);
        }
    }
}
