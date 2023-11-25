using Microsoft.Extensions.DependencyInjection;
using Moq;
using OpenTabletDriver.Components;
using OpenTabletDriver.Daemon;
using Xunit;

namespace OpenTabletDriver.Tests
{
    public class StrictServiceCollectionTest
    {
        [Fact]
        public void RequiredServices_AlwaysExist()
        {
            var serviceCollection = Utility.GetServices();

            var reportParserProvider = serviceCollection.BuildServiceProvider()
                .GetService<IReportParserProvider>();

            Assert.NotNull(reportParserProvider);
        }

        [Fact]
        public void RequiredServices_CanBeReplaced()
        {
            var stubReportParserProvider = new Mock<IReportParserProvider>().Object;
            var serviceCollection = new DesktopServiceCollection().AddSingleton(stubReportParserProvider);

            var retrievedReportParserProvider = serviceCollection.BuildServiceProvider()
                .GetService<IReportParserProvider>();

            Assert.Equal(stubReportParserProvider, retrievedReportParserProvider);
        }
    }
}
