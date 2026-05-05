using System.Linq;
using OpenTabletDriver.Configurations.Parsers.UCLogic;
using OpenTabletDriver.Desktop.Binding;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Tests.ConfigurationTest;
using Xunit;

namespace OpenTabletDriver.Tests.Binding
{
    public class GaomonM6WheelBindingTests
    {
        [Fact]
        public void GaomonM6_Configures_One_Twelve_Step_Absolute_Wheel()
        {
            var configuration = GetGaomonM6Configuration();
            var wheel = configuration.Specifications.Wheels!.Single();

            Assert.Equal(11u, wheel.AbsoluteWheelMax!.Value);
            Assert.Null(wheel.RelativeWheelSteps);
            Assert.Equal(12u, wheel.StepCount!.Value);
            Assert.Equal(0u, wheel.ButtonCount);
        }

        [Fact]
        public void GaomonM6_Uses_Wheel_Parser_For_All_Digitizer_Identifiers()
        {
            var configuration = GetGaomonM6Configuration();

            Assert.All(configuration.DigitizerIdentifiers, identifier =>
                Assert.Equal(typeof(UCLogicV2WheelReportParser).FullName, identifier.ReportParser));
        }

        [Fact]
        public void GaomonM6_RawWheelReports_Invoke_Clockwise_Binding()
        {
            var parser = new UCLogicV2WheelReportParser();
            var bindingHandler = CreateGaomonM6BindingHandler();
            var binding = new CountingBinding();
            bindingHandler.Wheels[0].ClockwiseRotation = new DeltaThresholdBindingState
            {
                Binding = binding,
                ActivationThreshold = 30,
                IsNegativeThreshold = false
            };

            bindingHandler.HandleBinding(parser.Parse(CreateGaomonM6WheelReport(0x0C)));
            bindingHandler.HandleBinding(parser.Parse(CreateGaomonM6WheelReport(0x0B)));

            Assert.Equal(1, binding.Presses);
        }

        [Fact]
        public void GaomonM6_RawWheelReports_Invoke_CounterClockwise_Binding()
        {
            var parser = new UCLogicV2WheelReportParser();
            var bindingHandler = CreateGaomonM6BindingHandler();
            var binding = new CountingBinding();
            bindingHandler.Wheels[0].CounterClockwiseRotation = new DeltaThresholdBindingState
            {
                Binding = binding,
                ActivationThreshold = 30,
                IsNegativeThreshold = true
            };

            bindingHandler.HandleBinding(parser.Parse(CreateGaomonM6WheelReport(0x0B)));
            bindingHandler.HandleBinding(parser.Parse(CreateGaomonM6WheelReport(0x0C)));

            Assert.Equal(1, binding.Presses);
        }

        private static TabletConfiguration GetGaomonM6Configuration()
        {
            return TestData.DeviceConfigurationProvider.TabletConfigurations
                .Single(config => config.Name == "Gaomon M6");
        }

        private static BindingHandler CreateGaomonM6BindingHandler()
        {
            var configuration = GetGaomonM6Configuration();

            return new BindingHandler(new TabletReference(configuration, configuration.DigitizerIdentifiers));
        }

        private static byte[] CreateGaomonM6WheelReport(byte rawPosition) =>
        [
            0x08, 0xF0, 0x01, 0x01, 0x00, rawPosition,
            0x00, 0x00, 0x00, 0x00, 0x13, 0xF2
        ];

        private sealed class CountingBinding : IStateBinding
        {
            public int Presses { get; private set; }

            public void Press(TabletReference tablet, IDeviceReport report)
            {
                Presses++;
            }

            public void Release(TabletReference tablet, IDeviceReport report)
            {
            }
        }
    }
}
