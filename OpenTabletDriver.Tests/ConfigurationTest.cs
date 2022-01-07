using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Plugin.Components;
using Xunit;

namespace OpenTabletDriver.Tests
{
    public class ConfigurationTest
    {
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
    }
}
