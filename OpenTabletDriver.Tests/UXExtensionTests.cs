using System.Reflection;
using Eto.Drawing;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.UX;
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

        [Fact]
        public void CornerNames_Are_Correct()
        {
            var rect = new RectangleF(PointF.Empty, new SizeF(10, 10));
            var corners = rect.GetAreaCorners(0f);
            rect.Offset(-rect.Size / 2);

            Assert.Equal(new PointF(corners.TopLeft.X, corners.TopLeft.Y), rect.TopLeft);
            Assert.Equal(new PointF(corners.TopRight.X, corners.TopRight.Y), rect.TopRight);
            Assert.Equal(new PointF(corners.BottomLeft.X, corners.BottomLeft.Y), rect.BottomLeft);
            Assert.Equal(new PointF(corners.BottomRight.X, corners.BottomRight.Y), rect.BottomRight);
        }
    }
}
