using OpenTabletDriver.Configurations.Parsers.Huion;
using Xunit;

namespace OpenTabletDriver.Tests.ParserTest
{
    public class GianoReportParserTest
    {
        [Theory]
        [InlineData(0x80, false)]
        [InlineData(0x81, false)]
        [InlineData(0xA0, true)]
        [InlineData(0xA1, true)]
        public void ParsesEraserState(int status, bool expectedEraser)
        {
            byte[] data = new byte[12];
            data[0] = 0x08;
            data[1] = (byte)status;

            var parser = new GianoReportParser();
            var report = Assert.IsType<GianoReport>(parser.Parse(data));

            Assert.Equal(expectedEraser, report.Eraser);
        }
    }
}
