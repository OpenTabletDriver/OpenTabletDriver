using System;
using OpenTabletDriver.Plugin;
using Xunit;

namespace OpenTabletDriver.Tests.ExtensionTests
{
    public class PrintHex
    {
        [Theory]
        [InlineData(new byte[] { 0x00 }, "00", false, false, false)]
        [InlineData(new byte[] { 0x07 }, "07", false, false, false)]
        [InlineData(new byte[] { 0xa1, 0xa2 }, "a1a2", false, false, false)]
        [InlineData(new byte[] { 0xa1, 0xa2 }, "A1A2", true, false, false)]
        [InlineData(new byte[] { 0xa1, 0xa2 }, "a1 a2", false, false, true)]
        [InlineData(new byte[] { 0xa1, 0xa2 }, "A1 A2", true, false, true)]
        [InlineData(new byte[] { 0xa1, 0xa2 }, "{a1a2}", false, true, false)]
        [InlineData(new byte[] { 0xa1, 0xa2 }, "{A1A2}", true, true, false)]
        [InlineData(new byte[] { 0xa1, 0xa2 }, "{ A1 A2 }", true, true, true)]
        [InlineData(new byte[] { 0xa1, 0xa2 }, "{ a1 a2 }", false, true, true)]
        public void Conversions(byte[] input, string expected, bool upperCase, bool wrap, bool spaced)
        {
            Assert.Equal(expected, input.PrintHex(upperCase: upperCase, wrap: wrap, spaced: spaced));
        }
    }
}
