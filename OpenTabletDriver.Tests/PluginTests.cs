using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Tests
{
    [TestClass]
    public class PluginTests
    {
        [DataTestMethod]
        [DataRow("[0..16)", 16, false)]
        [DataRow("[0..16)", 0, true)]
        [DataRow("(0..16]", 16, true)]
        [DataRow("(0..16]", 0, false)]
        public void RangeTest(string rangeStr, int testValue, bool expectedResult)
        {
            var range = DetectionRange.Parse(rangeStr);
            var result = range.IsInRange(testValue);
            Debug.Assert(result == expectedResult);
        }
    }
}
