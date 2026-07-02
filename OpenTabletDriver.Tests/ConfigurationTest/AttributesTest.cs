using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace OpenTabletDriver.Tests.ConfigurationTest
{
    public partial class AttributesTest
    {
        private static class ConfigurationAttributes
        {
            /// <summary>
            /// Check for only known skippable tests being present in attribute value
            /// </summary>
            /// <param name="attributeValue">A comma-separated string of values relevant to the <c>SkipTests</c> key.
            /// If there is only 1 value the comma is allowed to be absent</param>
            /// <returns><c>true</c> if comma-separated string contains only valid <see cref="OpenTabletDriver.Tests.ConfigurationTest.TestTypes"/> names, otherwise <c>false</c></returns>
            internal static bool CheckSkipTests(string attributeValue) =>
                attributeValue.Split(',').All(value => value.All(v => !char.IsDigit(v)) && Enum.TryParse(value, out TestTypes val) && Enum.IsDefined(val));

            /// <summary>
            /// Checks whether the value of <c>libinputoverride</c> is valid.
            /// The only valid value here is basically only <c>1</c>, but the <c>generate-rules.sh</c> script
            /// (at this time of writing) seems to add the line if <c>libinputoverride</c> is greater than <c>0</c>.
            /// As it might be undefined behavior to use anything else, we want to ensure that this is exactly 1.
            /// </summary>
            /// <param name="attributeValue">The value of the <c>libinputoverride</c> dictionary key</param>
            /// <returns><c>true</c> if value is 1, otherwise <c>false</c></returns>
            internal static bool CheckLibInputOverride(string attributeValue) =>
                ushort.TryParse(attributeValue, out ushort val) && val == 1;

            /// <summary>
            /// Check if <c>Interface</c> key value falls within expected range
            /// </summary>
            /// <param name="attributeValue">The value of the <c>Interface</c> key</param>
            /// <returns><c>true</c> if string can be parsed as an <see cref="uint"/> and does not have leading zeroes</returns>
            internal static bool CheckInterface(string attributeValue) =>
                uint.TryParse(attributeValue, out var val)
                && ((val == 0 && attributeValue.Length == 1) || attributeValue[0] != '0');

            /// <summary>
            /// Check if <c>VerifiedLPI</c> key value falls within the expected range
            /// </summary>
            /// <param name="attributeValue">The value of the <c>VerifiedLPI</c> key</param>
            /// <returns><c>true</c> if string can be parsed as a non-zero <see cref="uint"/></returns>
            internal static bool CheckVerifiedLPI(string attributeValue) =>
                uint.TryParse(attributeValue, out uint val) && val != 0;

            /// <summary>
            /// Checks if <c>FeatureInitDelayMs</c> key value falls within the expected range
            /// </summary>
            /// <param name="attributeValue">The value of the <c>FeatureInitDelayMs</c> key</param>
            /// <returns><c>true</c> if string can be parsed as a non-zero <see cref="uint"/> and is <c>10000</c> or less</returns>
            internal static bool CheckFeatureInitDelayMs(string attributeValue) =>
                uint.TryParse(attributeValue, out uint val) && val is > 0 and <= 10000;

            /// <summary>
            /// Checks if <c>WinUsage</c> key value falls within the expected range
            /// </summary>
            /// <param name="attributeValue">The value of the <c>WinUsage</c> key</param>
            /// <returns><c>true</c> if string is a zero-padded string <see cref="uint"/> value that is exactly 2 characters</returns>
            internal static bool CheckWinUsage(string attributeValue) =>
                uint.TryParse(attributeValue, out _) && attributeValue.Length == 2;


            /// <summary>
            /// Checks if <c>HidReports</c> matches expected format
            /// </summary>
            /// <param name="attributeValue">The value of the <c>HidReports</c> key</param>
            /// <returns><c>true</c> if all HidReport values in string are <c>, </c> separated and follow the format <c>{byte:X2}:{ushort:X4}:{ushort:X4}</c></returns>
            internal static bool CheckHidReports(string attributeValue)
            {
                var splitHidReports = attributeValue.Split(", ");
                foreach (var hidReport in splitHidReports)
                {
                    var splitHidReport = hidReport.Split(":");
                    if (splitHidReport.Length != 3)
                        return false;

                    if (splitHidReport[0].Length != 2 || splitHidReport[1].Length != 4 || splitHidReport[2].Length != 4)
                        return false;

                    if (!byte.TryParse(splitHidReport[0], System.Globalization.NumberStyles.HexNumber, null, out _)
                        || !ushort.TryParse(splitHidReport[1], System.Globalization.NumberStyles.HexNumber, null, out _)
                        || !ushort.TryParse(splitHidReport[2], System.Globalization.NumberStyles.HexNumber, null, out _))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        [Theory, MemberData(nameof(TestData.TestTabletConfigurations), MemberType = typeof(TestData))]
        public void Configurations_Have_Valid_Attributes(TestTabletConfiguration ttc)
        {
            var errors = new List<string>();

            var conf = ttc.Configuration.Value;
            var attribs = ExtractValues(conf.Attributes)
                .Concat(conf.DigitizerIdentifiers.SelectMany(x => ExtractValues(x.Attributes)));

            foreach (var attrib in attribs)
            {
                try
                {
                    var func = ValidConfigAttributes.SafeGet(pairs => pairs.Where(x => x.Key == attrib.Key).Select(x => x.Value),
                        null)?.FirstOrDefault();

                    if (func == null)
                    {
                        errors.Add($"Unknown attribute key '{attrib.Key}'");
                        continue;
                    }

                    if (!func(attrib.Value))
                        errors.Add($"Validation failed on '{attrib.Key}': '{attrib.Value}' is an invalid value");
                }
                catch (Exception ex)
                {
                    errors.Add($"Validating attribute '{attrib.Key}' threw an exception: {ex.Message}");
                }
            }

            string errorsFormatted = string.Join(Environment.NewLine, errors);
            Assert.True(errors.Count == 0, $"Errors detected in {ttc.FileShortName}:{Environment.NewLine}{errorsFormatted}");

            return;

            Dictionary<string, string> ExtractValues(Dictionary<string, string>? dict)
            {
                if (dict == null) return new Dictionary<string, string>();
                return dict;
            }
        }

    }
}
