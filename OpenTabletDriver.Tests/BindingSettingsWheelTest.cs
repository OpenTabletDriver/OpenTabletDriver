using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Plugin.Tablet;
using Xunit;

namespace OpenTabletDriver.Tests
{
    public class BindingSettingsWheelTest
    {
        // 7 positions, so one step is 360 / 7 degrees
        private static WheelSpecifications AbsoluteWheel => new() { AbsoluteWheelMax = 6 };
        private const float AbsoluteWheelStep = (float)(360d / 7);

        // 24 steps, so one step is 15 degrees
        private static WheelSpecifications RelativeWheel => new() { RelativeWheelSteps = 24 };
        private const float RelativeWheelStep = 15f;

        [Fact]
        public void Wheel_Added_To_Existing_Profile_Gets_One_Step_Threshold()
        {
            // A profile saved before the tablet's configuration declared a wheel
            var bindingSettings = new BindingSettings();

            bindingSettings.MatchSpecifications(CreateSpecifications(AbsoluteWheel));

            var wheel = Assert.Single(bindingSettings.WheelBindings);
            Assert.Equal(AbsoluteWheelStep, wheel.ClockwiseActivationThreshold);
            Assert.Equal(AbsoluteWheelStep, wheel.CounterClockwiseActivationThreshold);
        }

        [Fact]
        public void Wheels_Added_To_Existing_Profile_Get_Their_Own_Step_Threshold()
        {
            var bindingSettings = new BindingSettings();

            bindingSettings.MatchSpecifications(CreateSpecifications(AbsoluteWheel, RelativeWheel));

            Assert.Equal(2, bindingSettings.WheelBindings.Count);
            Assert.Equal(AbsoluteWheelStep, bindingSettings.WheelBindings[0].ClockwiseActivationThreshold);
            Assert.Equal(RelativeWheelStep, bindingSettings.WheelBindings[1].ClockwiseActivationThreshold);
        }

        [Theory]
        [InlineData(0f)]
        [InlineData(-5f)]
        public void Invalid_Threshold_Falls_Back_To_One_Step(float invalidThreshold)
        {
            var bindingSettings = new BindingSettings();
            bindingSettings.WheelBindings.Add(new WheelBindingSettings
            {
                ClockwiseActivationThreshold = invalidThreshold,
                CounterClockwiseActivationThreshold = invalidThreshold,
            });

            bindingSettings.MatchSpecifications(CreateSpecifications(AbsoluteWheel));

            var wheel = Assert.Single(bindingSettings.WheelBindings);
            Assert.Equal(AbsoluteWheelStep, wheel.ClockwiseActivationThreshold);
            Assert.Equal(AbsoluteWheelStep, wheel.CounterClockwiseActivationThreshold);
        }

        [Fact]
        public void Valid_Threshold_Is_Kept()
        {
            var bindingSettings = new BindingSettings();
            bindingSettings.WheelBindings.Add(new WheelBindingSettings
            {
                ClockwiseActivationThreshold = 1f,
                CounterClockwiseActivationThreshold = 100f,
            });

            bindingSettings.MatchSpecifications(CreateSpecifications(AbsoluteWheel));

            var wheel = Assert.Single(bindingSettings.WheelBindings);
            Assert.Equal(1f, wheel.ClockwiseActivationThreshold);
            Assert.Equal(100f, wheel.CounterClockwiseActivationThreshold);
        }

        [Fact]
        public void New_Profile_Gets_One_Step_Threshold()
        {
            var bindingSettings = BindingSettings.GetDefaults(CreateSpecifications(AbsoluteWheel));

            var wheel = Assert.Single(bindingSettings.WheelBindings);
            Assert.Equal(AbsoluteWheelStep, wheel.ClockwiseActivationThreshold);
            Assert.Equal(AbsoluteWheelStep, wheel.CounterClockwiseActivationThreshold);
        }

        private static TabletSpecifications CreateSpecifications(params WheelSpecifications[] wheels) => new()
        {
            Digitizer = new DigitizerSpecifications(),
            Pen = new PenSpecifications(),
            Wheels = [.. wheels],
        };
    }
}
