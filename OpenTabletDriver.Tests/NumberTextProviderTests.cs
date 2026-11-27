using OpenTabletDriver.UX.Controls.Generic.Text.Providers;
using Xunit;

namespace OpenTabletDriver.Tests
{
    public class NumberTextProviderTests
    {
        [Theory]
        [InlineData("1.5", "1.5")]
        [InlineData("1,5", "1.5")]
        [InlineData("-1,5", "-1.5")]
        [InlineData(",5", ".5")]
        [InlineData("-,5", "-.5")]
        [InlineData("1.2.3", "12.3")]
        [InlineData("1,2,3", "12.3")]
        [InlineData("1,234", "1.234")]
        [InlineData("1.234", "1.234")]
        [InlineData("1,234.5", "1234.5")]
        [InlineData("1.234,5", "1234.5")]
        public void Text_Normalizes_Decimal_Separators(string input, string expected)
        {
            var provider = new TestNumberTextProvider { Text = input };

            Assert.Equal(expected, provider.Text);
            Assert.Equal(expected, provider.DisplayText);
        }

        [Theory]
        [InlineData("15", 1, false, "1.5", 2)]
        [InlineData("1.5", 3, false, "15.", 3)]
        [InlineData("1.5", 0, false, ".15", 1)]
        [InlineData("-15", 0, false, "-.15", 2)]
        [InlineData("125", 1, true, "1.5", 2)]
        [InlineData("1.5", 3, true, "15.", 3)]
        [InlineData("1.5", 0, true, ".5", 1)]
        public void Editing_Comma_Preserves_Decimal_Position(string text, int position, bool replace, string expected, int expectedPosition)
        {
            var provider = new TestNumberTextProvider { Text = text };

            bool accepted = replace ? provider.Replace(',', ref position) : provider.Insert(',', ref position);

            Assert.True(accepted);
            Assert.Equal(expected, provider.Text);
            Assert.Equal(expectedPosition, position);
        }

        private class TestNumberTextProvider : NumberTextProvider<double>
        {
            public override double Value { get; set; }
        }
    }
}
