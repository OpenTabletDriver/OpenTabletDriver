using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using OpenTabletDriver.Plugin;
using Xunit;

namespace OpenTabletDriver.Tests.ExtensionTests
{
    public class SHA256Helpers
    {
        [Fact]
        public void StreamPositionIsKept()
        {
            var ms = new MemoryStream();
            ms.Write("this is a test"u8);
            var oldPos = ms.Position;
            _ = ms.GetSHA256();
            Assert.Equal(oldPos, ms.Position);
        }

        [Theory]
        [InlineData("", "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855")]
        [InlineData("test", "9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08")]
        [InlineData("last one", "c376d7c11bf250e5be75d858d47be8dd01779ed6dd24d5e7e8646937c1fc1da9")]
        public void CalculatesAndVerifiesHashFromString(string input, string expected)
        {
            var ms = new MemoryStream();
            ReadOnlySpan<byte> inputSpan = Encoding.ASCII.GetBytes(input);

            ms.Write(inputSpan);

            Assert.True(ms.VerifySHA256(expected, out var returned),
                $"expected {expected} != returned {returned.PrintHex()}");

            ReadOnlySpan<byte> inputHash = SHA256.HashData(inputSpan);
            Assert.Equal(inputHash, returned);
        }
    }
}
