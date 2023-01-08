using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OpenTabletDriver.Analyzers.Tests.Verifiers;
using Xunit;

namespace OpenTabletDriver.Analyzers.Tests
{
    public class TabletConfigurationGeneratorTests
    {
        [Theory]
        [InlineData("SingleEndpointTablet")]
        [InlineData("MultiEndpointTablet")]
        public Task Test(string tabletName)
        {
            GetResources(tabletName,
                out var sources,
                out var tabletJsonFiles,
                out var generatedSources,
                out var analyzerConfigOptions);

            return TabletConfigurationVerifier.Verify(sources, tabletJsonFiles, generatedSources, analyzerConfigOptions);
        }

        [Fact]
        public Task TestAllConfigurations()
        {
            GetResources("SingleEndpointTablet", out var sources, out _, out _, out _);

            var tabletJsonFiles = GetAllConfigurations();
            static string analyzerConfigOptionsFactory(string _) => "build_metadata.AdditionalFiles.TabletConfiguration = true";

            return TabletConfigurationVerifier.Verify(sources, tabletJsonFiles, analyzerConfigOptionsFactory);
        }

        private static void GetResources(string resourceName,
            out (string file, string content)[] sources,
            out (string file, string content)[] tabletJsonFiles,
            out (string file, string content)[] generatedFiles,
            out (string file, string content)[] analyzerConfigOptions)
        {
            var testResources = TestResourceHelper.GetGroupedTestResourcesContent(resourceName);

            sources = testResources
                .Single(g => g.Key == "Sources")
                .Where(f => f.file.EndsWith(".cs"))
                .ToArray();

            tabletJsonFiles = testResources
                .Single(g => g.Key == "Configurations")
                .Where(f => f.file.EndsWith(".json"))
                .ToArray();

            generatedFiles = testResources
                .Single(g => g.Key == "Generated")
                .Where(f => f.file.EndsWith(".cs"))
                .ToArray();

            analyzerConfigOptions = testResources
                .Single(g => g.Key == "AnalyzerConfigOptions")
                .Where(f => f.file.EndsWith(".editorconfig"))
                .Select(f => (Path.GetFileNameWithoutExtension(f.file), f.content))
                .ToArray();
        }

        private static (string file, string content)[] GetAllConfigurations()
        {
            var configurationProjectDir = Path.GetFullPath(Path.Join(TestResourceHelper.TestProjectDir, "..", "OpenTabletDriver.Configurations"));
            var configurationDir = Path.Join(configurationProjectDir, "Configurations");

            return Directory.EnumerateFiles(configurationDir, "*.json", SearchOption.AllDirectories)
                .Select(f => (Path.GetFileName(f), File.ReadAllText(f)))
                .ToArray();
        }
    }
}
