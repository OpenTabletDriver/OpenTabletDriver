using System;
using OpenTabletDriver.Plugin;
using Xunit;

namespace OpenTabletDriver.Tests.ExtensionTests
{
    public class StringElision
    {
        [Theory]
        [InlineData("", "", 100)]
        [InlineData("not long enough", "not long enough", 100)]
        [InlineData("don't cut me off", "do...", 5)]
        public void ElisionLength_Is_MaxLength(string input, string expected, int elisionLength)
        {
            Assert.Equal(expected, input.Elide(elisionLength));
        }

        [Fact]
        public void Has_Sane_Defaults()
        {
            Assert.Equal("...", "myString".Elide(3));
        }

        [Theory]
        [InlineData("Hello There", "H-----", 6, "-----")]
        [InlineData("Okay", "Okay", 9, "-----")]
        [InlineData("pyrotechnics", "pyrosulfate", 11, "sulfate")]
        public void Marker_Length_Is_Accounted_For(string input, string expected, int elisionLength, string elisionMarker)
        {
            Assert.Equal(expected, input.Elide(elisionLength, elisionMarker));
        }

        [Fact]
        public void Throws_ArgumentOutOfRangeException_If_Invalid()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => "".Elide(5, "very long marker"));
            Assert.Throws<ArgumentOutOfRangeException>(() => "".Elide(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => "".Elide(-1));
        }

        [Fact]
        public void Passes_On_Empty_ElisionMarker_With_ZeroLength_Elision()
        {
            Assert.Equal("", "myString".Elide(0, string.Empty));
        }
    }
}
