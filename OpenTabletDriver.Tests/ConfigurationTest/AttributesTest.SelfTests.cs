using System;
using System.Collections.Generic;
using OpenTabletDriver.Plugin.Tablet;
using Xunit;

namespace OpenTabletDriver.Tests.ConfigurationTest
{
    public partial class AttributesTest
    {
        [Theory]
        // obviously invalid
        [InlineData("invalid", false)]
        [InlineData("LPI_LOOKS_LIKE_A_VALID_OPTION,BREAD,BREAD", false)]
        [InlineData("", false)]
        [InlineData("0", false)]
        [InlineData("1", false)]
        [InlineData("0,1", false)]
        [InlineData("1,0", false)]
        [InlineData("1,0,", false)]
        [InlineData("1234", false)]
        // mixed valid and invalid values
        [InlineData($"{nameof(TestTypes.LPI_DIGITIZER_X)},invalid", false)]
        // trailing comma should be considered invalid
        [InlineData($"{nameof(TestTypes.LPI_DIGITIZER_X)},{nameof(TestTypes.LPI_DIGITIZER_Y)},", false)]
        // definitely valid
        [InlineData(nameof(TestTypes.LPI_DIGITIZER_X), true)]
        [InlineData($"{nameof(TestTypes.LPI_DIGITIZER_X)},{nameof(TestTypes.LPI_DIGITIZER_Y)}", true)]
        [InlineData($"{nameof(TestTypes.LPI_DIGITIZER_Y)},{nameof(TestTypes.LPI_DIGITIZER_X)}", true)]
        [InlineData($"{nameof(TestTypes.LPI_SAME_ACROSS_AXES)},{nameof(TestTypes.LPI_DIGITIZER_Y)},{nameof(TestTypes.LPI_DIGITIZER_X)}", true)]
        public static void Configurations_SelfTest_Attributes_SkipTest(string attributeValue, bool expectedStatus) =>
            Assert.Equal(ConfigurationAttributes.CheckSkipTests(attributeValue), expectedStatus);

        [Theory]
        [InlineData("invalid", false)]
        [InlineData("2", false)]
        [InlineData("-1", false)]
        [InlineData("0", false)]
        [InlineData("1", true)]
        public static void Configurations_SelfTest_Attributes_LibInputOverride(string attributeValue, bool expectedStatus) =>
            Assert.Equal(ConfigurationAttributes.CheckLibInputOverride(attributeValue), expectedStatus);

        [Theory]
        [InlineData("00", false)]
        [InlineData("02", false)]
        [InlineData("-1", false)]
        [InlineData("invalid", false)]
        [InlineData("0", true)]
        [InlineData("1", true)]
        [InlineData("2", true)]
        [InlineData("3", true)]
        public static void Configurations_SelfTest_Attributes_Interface(string attributeValue, bool expectedStatus) =>
            Assert.Equal(ConfigurationAttributes.CheckInterface(attributeValue), expectedStatus);

        [Theory]
        [InlineData("-1", false)]
        [InlineData("invalid", false)]
        [InlineData("0", false)]
        [InlineData("1", true)]
        [InlineData("2", true)]
        [InlineData("3", true)]
        [InlineData("400", true)]
        [InlineData("1234", true)]
        [InlineData("6900", true)]
        [InlineData("25600", true)]
        public static void Configurations_SelfTest_Attributes_VerifiedLPI(string attributeValue, bool expectedStatus) =>
            Assert.Equal(ConfigurationAttributes.CheckVerifiedLPI(attributeValue), expectedStatus);

        [Theory]
        [InlineData("-1", false)]
        [InlineData("invalid", false)]
        [InlineData("00", false)]
        [InlineData("0", false)]
        [InlineData("1", true)]
        [InlineData("10", true)]
        [InlineData("100", true)]
        [InlineData("1000", true)]
        [InlineData("3000", true)]
        [InlineData("10000", true)]
        public static void Configurations_SelfTest_Attributes_FeatureInitDelayMs(string attributeValue, bool expectedStatus) =>
            Assert.Equal(ConfigurationAttributes.CheckFeatureInitDelayMs(attributeValue), expectedStatus);

        [Theory]
        [InlineData("-1", false)]
        [InlineData("invalid", false)]
        [InlineData("0", false)]
        [InlineData("1", false)]
        [InlineData("00", true)]
        [InlineData("01", true)]
        [InlineData("09", true)]
        public static void Configurations_SelfTest_Attributes_WinUsage(string attributeValue, bool expectedStatus) =>
            Assert.Equal(ConfigurationAttributes.CheckWinUsage(attributeValue), expectedStatus);

        /// <summary>
        /// Key is the attribute key from the <see cref="TabletConfiguration.Attributes"/> dictionary.
        /// Value is a function pointer that parses the attribute value string and returns true if valid
        /// </summary>
        private static IEnumerable<KeyValuePair<string, Func<string, bool>?>> ValidConfigAttributes
        {
            get
            {
                yield return new(Consts.SKIP_TESTS_ATTRIBUTE_KEY, ConfigurationAttributes.CheckSkipTests);
                yield return new("libinputoverride", ConfigurationAttributes.CheckLibInputOverride);
                yield return new("Interface", ConfigurationAttributes.CheckInterface);
                yield return new(Consts.VERIFIED_LPI_KEY, ConfigurationAttributes.CheckVerifiedLPI);
                yield return new("FeatureInitDelayMs", ConfigurationAttributes.CheckFeatureInitDelayMs);
                yield return new("WinUsage", ConfigurationAttributes.CheckWinUsage);
                yield return new("HidReports", ConfigurationAttributes.CheckHidReports);
            }
        }

    }
}
