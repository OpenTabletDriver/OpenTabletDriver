using System;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Components;
using OpenTabletDriver.Configurations.Parsers.XP_Pen;
using OpenTabletDriver.Tablet;
using Xunit;

namespace OpenTabletDriver.Tests
{
    public class ReportParserProviderTest
    {
        public static TheoryData<string, Type> ReportParserProvider_CanGet_ReportParsers_Data => new TheoryData<string, Type>
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
            var serviceCollection = Utility.GetServices();
            var reportParserProvider = serviceCollection.BuildServiceProvider()
                .GetRequiredService<IReportParserProvider>();

            var reportParserType = reportParserProvider.GetReportParser(reportParserName).GetType();

            Assert.Equal(expectedReportParserType, reportParserType);
        }
    }
}
