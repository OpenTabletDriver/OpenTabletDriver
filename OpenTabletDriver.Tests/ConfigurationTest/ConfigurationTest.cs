using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using OpenTabletDriver.Plugin.Tablet;
using Xunit;
using Xunit.Abstractions;

namespace OpenTabletDriver.Tests.ConfigurationTest
{
    public class ConfigurationTest(ITestOutputHelper testOutputHelper)
    {
        [Theory]
        [MemberData(nameof(TestData.ParsersInConfigs), MemberType = typeof(TestData))]
        public void Configurations_Have_ExistentParsers(string parserName)
        {
            TestData.ReportParserProvider.GetReportParser(parserName);
        }

        [Theory]
        [MemberData(nameof(TestData.TestTabletConfigurations), MemberType = typeof(TestData))]
        public void Configurations_Verify_Configs_With_Schema(TestTabletConfiguration testTabletConfiguration)
        {
            var tabletFilename = testTabletConfiguration.File.Name;
            var tabletConfigString = testTabletConfiguration.FileContents.Value;
            var schema = TestData.TabletConfigurationSchema;
            IList<string> errors = new List<string>();

            var tabletConfig = JObject.Parse(tabletConfigString);
            try
            {
                Assert.True(tabletConfig.IsValid(schema, out errors));
            }
            catch (Exception)
            {
                if (errors.Any())
                    testOutputHelper.WriteLine($"Schema errors in {tabletFilename}: " + string.Join(",", errors));

                throw;
            }
        }

        /// <summary>
        /// Ensures that configuration formatting/linting matches expectations, which are:
        /// - 2 space indentation
        /// - Newline at end of file
        /// - Consistent newline format
        /// </summary>
        [Theory]
        [MemberData(nameof(TestData.TestTabletConfigurations), MemberType = typeof(TestData))]
        public void Configurations_Are_Linted(TestTabletConfiguration ttc)
        {
            var currentContent = ttc.FileContents.Value;

            var serializer = new JsonSerializer()
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            var sb = new StringBuilder();
            using var strw = new StringWriter(sb);
            using var jtw = new JsonTextWriter(strw);

            jtw.Formatting = Formatting.Indented;
            jtw.Indentation = 2;

            var ourJsonObj = JsonConvert.DeserializeObject<TabletConfiguration>(currentContent);
            serializer.Serialize(jtw, ourJsonObj);
            sb.AppendLine();
            var expected = sb.ToString();

            var diff = InlineDiffBuilder.Diff(currentContent, expected, ignoreWhiteSpace: false);

            if (diff.HasDifferences)
            {
                testOutputHelper.WriteLine($"'{ttc.File.Name}' did not match linting:");
                PrintDiff(testOutputHelper, diff);
                Assert.True(false);
            }
        }

        private static readonly Regex AvaloniaReportParserPath = TestData.AvaloniaReportParserPathRegex();

        [Theory]
        [MemberData(nameof(TestData.TestTabletConfigurations), MemberType = typeof(TestData))]
        public void Configurations_Have_No_Legacy_Properties(TestTabletConfiguration ttc)
        {
            var errors = new List<string>();

            var config = ttc.Configuration.Value;
            string filePath = ttc.FileShortName;

            // disable warning for "obsoleted" paths
#pragma warning disable CS0618 // Type or member is obsolete
            // aux identifier rename
            if (config.HasLegacyProperties())
                errors.Add("Incorrect key AuxilaryDeviceIdentifiers is present. It should be 'AuxiliaryDeviceIdentifiers'");

            // pen ButtonCount rename and type change
            if (config.Specifications.Pen.HasLegacyProperties())
                errors.Add("Incorrect key Specifications.Pen.Buttons is present. The Pen.Buttons.ButtonCount value should be moved to Pen.ButtonCount");
#pragma warning restore CS0618 // Type or member is obsolete

            // ReportParser path change is also indirectly tested elsewhere (as the class path won't exist on this version)
            // but it's still a good idea to test here, in case this test is run by itself
            if (config.DigitizerIdentifiers.Any(x => AvaloniaReportParserPath.IsMatch(x.ReportParser)))
                errors.Add("0.7-only ReportParser path detected. Replace all ReportPath instances of 'OpenTabletDriver.Tablet.' with 'OpenTabletDriver.Plugin.Tablet.'");

            string errorsFormatted = string.Join(Environment.NewLine, errors);
            Assert.True(errors.Count == 0, $"Errors detected in {filePath}:{Environment.NewLine}{errorsFormatted}");
        }

        private const decimal MILLIMETERS_PER_INCH = 25.4m;

        // touch untested
        [SkippableTheory]
        [MemberData(nameof(TestData.TestTabletConfigurations), MemberType = typeof(TestData))]
        public void Configurations_Have_Predictable_Digitizer_Dimensions(TestTabletConfiguration ttc)
        {
            var errors = new List<string>();

            var digitizer = ttc.Configuration.Value.Specifications.Digitizer;
            string filePath = ttc.FileShortName;

            bool skipXTest = ttc.SkippedTestTypes.Contains(TestTypes.LPI_DIGITIZER_X);
            bool skipYTest = ttc.SkippedTestTypes.Contains(TestTypes.LPI_DIGITIZER_Y);
            bool skipAxisEqualTest = ttc.SkippedTestTypes.Contains(TestTypes.LPI_SAME_ACROSS_AXES);

            Skip.If(skipYTest && skipXTest && skipAxisEqualTest, "All LPI checks requested skipped");

            int maxX = (int)digitizer.MaxX;
            decimal width = digitizer.WidthAsDecimal;
            decimal? lpiXResult = null;

            if (!skipXTest)
            {
                decimal widthInches = width / MILLIMETERS_PER_INCH;
                decimal lpiX = maxX / widthInches;
                lpiXResult = validateLPI(lpiX, width, maxX, nameof(width), ttc.ValidLPIsForTablet);
            }

            int maxY = (int)digitizer.MaxY;
            decimal height = digitizer.HeightAsDecimal;
            decimal? lpiYResult = null;

            if (!skipYTest)
            {
                decimal heightInches = height / MILLIMETERS_PER_INCH;
                decimal lpiY = maxY / heightInches;
                lpiYResult = validateLPI(lpiY, height, maxY, nameof(height), ttc.ValidLPIsForTablet);
            }

            if (lpiYResult.HasValue && lpiXResult.HasValue && lpiYResult.Value != lpiXResult.Value)
                errors.Add("Note that the assumed LPI's did not match across axes!");

            if (!skipAxisEqualTest)
            {
                decimal lpmmY = maxY / height;
                decimal lpmmX = maxX / width;

                decimal millimetersPerXLine = 1 / lpmmX;
                decimal millimetersPerYLine = 1 / lpmmY;

                decimal diff = Math.Abs(lpmmX - lpmmY);

                if (diff >= millimetersPerYLine || diff >= millimetersPerXLine)
                    errors.Add($"X lpmm does not match Y lpmm. X: {lpmmX:0.##}, Y: {lpmmY:0.##}");
            }

            string errorsFormatted = string.Join(Environment.NewLine, errors);
            Assert.True(errors.Count == 0, $"Errors detected in {filePath}:{Environment.NewLine}{errorsFormatted}");
            return;

            decimal validateLPI(decimal lpi, decimal size, decimal maxLines, string physicalSide, IEnumerable<int> validLPIs)
            {
                decimal? closestLpi = null;

                var validLPIsArr = validLPIs as int[] ?? validLPIs.ToArray();
                foreach (decimal validLpi in validLPIsArr.OrderBy(x => x))
                {
                    if (closestLpi == null)
                    {
                        closestLpi = validLpi;
                        continue;
                    }

                    if (Math.Abs(validLpi - lpi) <= Math.Abs(closestLpi.Value - lpi))
                        closestLpi = validLpi;
                }

                Debug.Assert(closestLpi.HasValue);

                decimal suggestedSize = (maxLines / closestLpi.Value) * MILLIMETERS_PER_INCH;
                suggestedSize = Math.Round(suggestedSize, 8);

                decimal millimetersPerLine = 1 / (closestLpi.Value / MILLIMETERS_PER_INCH);

                string capitalizedPhysicalSide = string.Concat(physicalSide[0].ToString().ToUpper(), physicalSide.AsSpan(1));

                // only emit error if width/height is 1 unit or more off
                if (Math.Abs(size - suggestedSize) >= millimetersPerLine)
                    errors.Add(
                        $"Unexpected {physicalSide} LPI {lpi:0.##}. Must be one of {string.Join(", ", validLPIsArr)}. Assuming an LPI of {closestLpi}, {capitalizedPhysicalSide} '{size}' needs to be '{suggestedSize:0.#####}' instead.");

                return closestLpi.Value;
            }
        }

        private static void PrintDiff(ITestOutputHelper outputHelper, DiffPaneModel diff)
        {
            foreach (var line in diff.Lines)
            {
                switch (line.Type)
                {
                    case ChangeType.Inserted:
                        outputHelper.WriteLine($"+ {line.Text}");
                        break;
                    case ChangeType.Deleted:
                        outputHelper.WriteLine($"- {line.Text}");
                        break;
                    default:
                        outputHelper.WriteLine($"  {line.Text}");
                        break;
                }
            }
        }

    }
}
