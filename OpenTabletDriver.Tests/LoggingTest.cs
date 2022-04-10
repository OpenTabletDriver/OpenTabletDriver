using System.IO;
using OpenTabletDriver.Plugin;
using Xunit;

namespace OpenTabletDriver.Tests
{
    public class LoggingTest
    {
        [Fact]
        public void Ignore_Write_On_Log_Level_Below_Verbosity()
        {
            var fired = false;
            Log.Output += (_, _) => fired = true;
            Log.Verbosity = LogLevel.Warning;

            Log.Write("Test", "Should not fire", LogLevel.Info);
            Assert.False(fired);

            Log.Write("Test", "Should fire", LogLevel.Warning);
            Assert.True(fired);
        }
    }
}
