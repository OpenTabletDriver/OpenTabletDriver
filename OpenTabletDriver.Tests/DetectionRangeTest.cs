using OpenTabletDriver.Plugin.Tablet;
using Xunit;

namespace OpenTabletDriver.Tests
{
    public class DetectionRangeTest
    {
        [Theory]
        [InlineData("[0..16)", 16, false)]
        [InlineData("[0..16)", 0, true)]
        [InlineData("(0..16]", 16, true)]
        [InlineData("(0..16]", 0, false)]
        public void RangeTest(string rangeStr, int testValue, bool expectedResult)
        {
            var range = DetectionRange.Parse(rangeStr);

            var result = range.IsInRange(testValue);

            Assert.Equal(expectedResult, result);
        }
    }
}
