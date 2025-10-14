using System.Collections.Generic;
using System.Linq;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Wheel;

#nullable enable

namespace OpenTabletDriver.Desktop.Binding
{
    public class WheelBindingHandler
    {
        public WheelBindingHandler(TabletReference tablet, int index = 0)
        {
            this.tablet = tablet;

            if (tablet.Properties.Specifications.Wheels == null || tablet.Properties.Specifications.Wheels.Count <= index) return;

            this.wheelSteps = tablet.Properties.Specifications.Wheels[index]?.StepCount ?? 0;
            this.halfWheelSteps = tablet.Properties.Specifications.Wheels[index]?.StepCount / 2d;
            this.threeHalfWheelSteps = this.halfWheelSteps * 3d;
        }

        public Dictionary<int, BindingState?> WheelButtons { set; get; } = new Dictionary<int, BindingState?>();

        public DeltaThresholdBindingState? ClockwiseRotation { set; get; }
        public DeltaThresholdBindingState? CounterClockwiseRotation { set; get; }

        public int Index { set; get; }

        private readonly uint? wheelSteps;
        private readonly double? halfWheelSteps;
        private readonly double? threeHalfWheelSteps;

        private uint? lastWheelPosition;
        private float currentWheelDelta;

        private readonly TabletReference tablet;

        public void HandleBinding(IDeviceReport report)
        {
            if (report is IAbsoluteWheelsReport absoluteWheelsReport)
                foreach (var position in absoluteWheelsReport.WheelsPosition)
                    HandleAbsoluteWheelReport(tablet, report, position);
            if (report is IRelativeWheelsReport relativeWheelsReport)
                foreach (var delta in relativeWheelsReport.WheelsDelta)
                    HandleRelativeWheelReport(tablet, report, delta);
            if (report is IWheelsButtonsReport wheelsButtonsReport)
                foreach (var wheel in wheelsButtonsReport.WheelsButtons)
                    HandleWheelButtons(tablet, report, wheel.States);
        }

        private void HandleWheelButtons(TabletReference tablet, IDeviceReport report, bool[] states)
        {
            HandleBindingCollection(tablet, report, WheelButtons, states);
        }

        private void HandleAbsoluteWheelReport(TabletReference tablet, IDeviceReport report, uint? position)
        {
            int? delta = ComputeWheelDelta(lastWheelPosition, position);
            HandleRelativeWheelReport(tablet, report, delta);
            lastWheelPosition = position;
        }

        private void HandleRelativeWheelReport(TabletReference tablet, IDeviceReport report, int? delta)
        {
            currentWheelDelta += delta ?? 0;

            ClockwiseRotation?.Invoke(tablet, report, ref currentWheelDelta);
            CounterClockwiseRotation?.Invoke(tablet, report, ref currentWheelDelta);

            // Some issues with keys staying pressed when holding on specific tablets
            // This will cause consistency issues on higher end machines (Basically all nowadays)
            ClockwiseRotation?.Invoke(tablet, report, false);
            CounterClockwiseRotation?.Invoke(tablet, report, false);
        }

        private int? ComputeWheelDelta(uint? from, uint? to)
        {
            return (int?)((((int?)to - from + threeHalfWheelSteps) % wheelSteps) - halfWheelSteps);
        }

        private static void HandleBindingCollection(TabletReference tablet, IDeviceReport report, IDictionary<int, BindingState?> bindings, IList<bool> newStates)
        {
            for (int i = 0; i < newStates.Count; i++)
            {
                if (bindings.TryGetValue(i, out var binding))
                    binding?.Invoke(tablet, report, newStates[i]);
            }
        }

        public override string ToString()
        {
            return $"Wheel n°{Index}, " +
                   $"Wheel Buttons Bindings: [{string.Join("], [", WheelButtons.Select(b => b.Value?.Binding))}], " +
                   $"Clockwise Rotation: [{ClockwiseRotation?.Binding}], Counter-Clockwise Rotation: [{CounterClockwiseRotation?.Binding}]";
        }
    }
}