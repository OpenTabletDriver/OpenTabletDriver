using System.Linq;
using System.Reflection;
using Xunit;

namespace OpenTabletDriver.Tests
{
    public class ResourceTests
    {
        [Fact]
        public void Configurations_Assembly_Contains_Json_Files()
        {
            var confAsm = Assembly.Load("OpenTabletDriver.Configurations");
            var jsonFiles = confAsm.GetManifestResourceNames().Where(x => x.EndsWith(".json"));
            Assert.True(jsonFiles.Any());
        }
    }
}
