using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.UX.Controls;
using Xunit;

namespace OpenTabletDriver.Tests
{
    public class WheelTabLayoutTests
    {
        [Fact]
        public void Create_WithPlainLegacyWheelArray_KeepsStandaloneWheelTabs()
        {
            var tabs = WheelTabLayout.Create(
            [
                CreateWheel(),
                CreateWheel()
            ]);

            Assert.Collection(tabs,
                first =>
                {
                    Assert.False(first.IsGrouped);
                    Assert.Equal("Wheel 1 Bindings", first.Text);
                    var wheel = Assert.Single(first.Wheels);
                    Assert.Equal(0, wheel.WheelIndex);
                    Assert.Equal("Wheel 1", wheel.Title);
                },
                second =>
                {
                    Assert.False(second.IsGrouped);
                    Assert.Equal("Wheel 2 Bindings", second.Text);
                    var wheel = Assert.Single(second.Wheels);
                    Assert.Equal(1, wheel.WheelIndex);
                    Assert.Equal("Wheel 2", wheel.Title);
                });
        }

        [Fact]
        public void Create_WithNamedUngroupedWheels_UsesWheelNamesForStandaloneTabs()
        {
            var tabs = WheelTabLayout.Create(
            [
                CreateWheel(name: "Scroll"),
                CreateWheel(name: "Zoom")
            ]);

            Assert.Collection(tabs,
                first => Assert.Equal("Scroll Bindings", first.Text),
                second => Assert.Equal("Zoom Bindings", second.Text));
        }

        [Fact]
        public void Create_WithGroupedWheels_CollapsesSharedGroupsByFirstAppearance()
        {
            var tabs = WheelTabLayout.Create(
            [
                CreateWheel(name: "Scroll", group: "Left Dial"),
                CreateWheel(name: "Scroll", group: "Right Dial"),
                CreateWheel(name: "Multimedia", group: "Left Dial"),
                CreateWheel(group: "Right Dial")
            ]);

            Assert.Collection(tabs,
                left =>
                {
                    Assert.True(left.IsGrouped);
                    Assert.Equal("Left Dial", left.Text);
                    Assert.Collection(left.Wheels,
                        first =>
                        {
                            Assert.Equal(0, first.WheelIndex);
                            Assert.Equal("Scroll", first.Title);
                        },
                        second =>
                        {
                            Assert.Equal(2, second.WheelIndex);
                            Assert.Equal("Multimedia", second.Title);
                        });
                },
                right =>
                {
                    Assert.True(right.IsGrouped);
                    Assert.Equal("Right Dial", right.Text);
                    Assert.Collection(right.Wheels,
                        first =>
                        {
                            Assert.Equal(1, first.WheelIndex);
                            Assert.Equal("Scroll", first.Title);
                        },
                        second =>
                        {
                            Assert.Equal(3, second.WheelIndex);
                            Assert.Equal("Wheel 4", second.Title);
                        });
                });
        }

        [Fact]
        public void Create_WithMixedGroupedAndUngroupedWheels_PreservesWheelOrder()
        {
            var tabs = WheelTabLayout.Create(
            [
                CreateWheel(name: "Wheel A"),
                CreateWheel(name: "Scroll", group: "Left Dial"),
                CreateWheel(name: "Multimedia", group: "Left Dial"),
                CreateWheel(name: "Wheel B"),
                CreateWheel(name: "Scroll", group: "Right Dial")
            ]);

            Assert.Collection(tabs,
                first =>
                {
                    Assert.False(first.IsGrouped);
                    Assert.Equal("Wheel A Bindings", first.Text);
                    Assert.Equal(0, Assert.Single(first.Wheels).WheelIndex);
                },
                second =>
                {
                    Assert.True(second.IsGrouped);
                    Assert.Equal("Left Dial", second.Text);
                    Assert.Collection(second.Wheels,
                        firstWheel => Assert.Equal(1, firstWheel.WheelIndex),
                        secondWheel => Assert.Equal(2, secondWheel.WheelIndex));
                },
                third =>
                {
                    Assert.False(third.IsGrouped);
                    Assert.Equal("Wheel B Bindings", third.Text);
                    Assert.Equal(3, Assert.Single(third.Wheels).WheelIndex);
                },
                fourth =>
                {
                    Assert.True(fourth.IsGrouped);
                    Assert.Equal("Right Dial", fourth.Text);
                    Assert.Equal(4, Assert.Single(fourth.Wheels).WheelIndex);
                });
        }

        [Fact]
        public void Create_WithWhitespaceGroup_TreatsWheelAsUngrouped()
        {
            var tabs = WheelTabLayout.Create(
            [
                CreateWheel(group: "  "),
                CreateWheel(name: "Scroll", group: "Left Dial")
            ]);

            Assert.Collection(tabs,
                first =>
                {
                    Assert.False(first.IsGrouped);
                    Assert.Equal("Wheel 1 Bindings", first.Text);
                },
                second =>
                {
                    Assert.True(second.IsGrouped);
                    Assert.Equal("Left Dial", second.Text);
                });
        }

        private static WheelSpecifications CreateWheel(string? name = null, string? group = null)
        {
            return new WheelSpecifications
            {
                AbsoluteWheelMax = 7,
                ButtonCount = 0,
                Name = name,
                Group = group
            };
        }
    }
}
