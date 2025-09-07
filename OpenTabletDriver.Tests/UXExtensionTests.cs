using System.Reflection;
using OpenTabletDriver.Plugin.Attributes;
using Xunit;
using static OpenTabletDriver.UX.Controls.Generic.Reflection.Extensions;

namespace OpenTabletDriver.Tests
{
    public static class PluginNameDefinition
    {
        public const string Name = "This is a plugin name";
    }

    [PluginName(PluginNameDefinition.Name)]
    public class PluginNameAttributeTest;

    public class UXExtensionTests
    {
        [Fact]
        public void GetFriendlyName_Reads_PluginNameAttribute()
        {
            var attributedType = new PluginNameAttributeTest();
            Assert.Equal(PluginNameDefinition.Name, attributedType.GetType().GetTypeInfo().GetFriendlyName());
        }

        [Fact]
        public void GetFriendlyName_Defaults_To_FullName()
        {
            var ns = typeof(UXExtensionTests).Namespace;
            var expected = $"{ns}.{nameof(UXExtensionTests)}";
            var output = typeof(UXExtensionTests).GetTypeInfo().GetFriendlyName();
            Assert.Equal(expected, output);
        }
    }
}
