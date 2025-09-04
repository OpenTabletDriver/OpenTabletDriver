using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Plugin.Tablet;
using Xunit;

namespace OpenTabletDriver.Tests.Data
{
    public record TestTabletConfiguration
    {
        public required Lazy<TabletConfiguration> Configuration { get; init; }
        public required FileInfo File { get; init; }
        public required Lazy<string> FileContents  { get; init; }
    }

    public static partial class ConfigurationTestData
    {
        public static TheoryData<TestTabletConfiguration> TestTabletConfigurations =>
            GetTestTabletConfigurations();

        private static TheoryData<TestTabletConfiguration> GetTestTabletConfigurations()
        {
            var result = new TheoryData<TestTabletConfiguration>();
            foreach (var configFile in Directory.EnumerateFiles(GetConfigDir(), "*.json", SearchOption.AllDirectories))
            {
                FileInfo configFileInfo = new FileInfo(configFile);
                var ttc = new TestTabletConfiguration
                {
                    Configuration = new Lazy<TabletConfiguration>(() => Deserialize(configFileInfo)),
                    File = configFileInfo,
                    FileContents = new Lazy<string>(() => configFileInfo.OpenText().ReadToEnd())
                };
                result.Add(ttc);
            }

            return result;
        }

        private static string GetConfigDir([CallerFilePath] string sourceFilePath = "") =>
            Path.GetFullPath(Path.Join(sourceFilePath, "../../../OpenTabletDriver.Configurations/Configurations"));

        private static TabletConfiguration Deserialize(FileInfo configFileInfo) => Serialization.Deserialize<TabletConfiguration>(configFileInfo);

        [GeneratedRegex(@"^OpenTabletDriver\.Tablet\..*$", RegexOptions.Compiled)]
        public static partial Regex AvaloniaReportParserPathRegex();
    }
}
