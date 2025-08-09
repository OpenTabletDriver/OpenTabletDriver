using System;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Configurations.Parsers.XP_Pen;
using OpenTabletDriver.Plugin.Components;
using OpenTabletDriver.Plugin.Tablet;
using Xunit;

namespace OpenTabletDriver.Tests
{
    public class ReportParserProviderTest
    {
        public static TheoryData<string, Type> ReportParserProvider_CanGet_ReportParsers_Data => new()
        {
            // Built-in
            { typeof(TabletReportParser).FullName!, typeof(TabletReportParser) },
            // OTD.Configurations
            { typeof(XP_PenReportParser).FullName!, typeof(XP_PenReportParser) }
        };

        [Theory]
        [MemberData(nameof(ReportParserProvider_CanGet_ReportParsers_Data))]
        public void ReportParserProvider_CanGet_ReportParsers(string reportParserName, Type expectedReportParserType)
        {
            var serviceCollection = new DriverServiceCollection();
            var reportParserProvider = serviceCollection.BuildServiceProvider()
                .GetRequiredService<IReportParserProvider>();

            var reportParserType = reportParserProvider.GetReportParser(reportParserName).GetType();

            Assert.Equal(expectedReportParserType, reportParserType);
        }
    }
}
