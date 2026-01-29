using System;
using System.Collections.Generic;
using OpenTabletDriver.Configurations.Parsers.Wacom.Intuos4;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Wheel;

#nullable enable

namespace OpenTabletDriver.Desktop.Binding
{
    [PluginIgnore]
    public class BindingHandler : IPositionedPipelineElement<IDeviceReport>
    {
        public BindingHandler(TabletReference tablet)
        {
            this.tablet = tablet;
            this.wheelSteps = tablet.Properties.Specifications.Wheel?.StepCount ?? 0;
            this.halfWheelSteps = tablet.Properties.Specifications.Wheel?.StepCount / 2d;
            this.threeHalfWheelSteps = this.halfWheelSteps * 3d;
        }

        public ThresholdBindingState? Tip { set; get; }
        public ThresholdBindingState? Eraser { set; get; }
        private bool _isEraser;

        public Dictionary<int, BindingState?> PenButtons { set; get; } = new Dictionary<int, BindingState?>();
        public Dictionary<int, BindingState?> AuxButtons { set; get; } = new Dictionary<int, BindingState?>();
        public Dictionary<int, BindingState?> MouseButtons { set; get; } = new Dictionary<int, BindingState?>();
        public Dictionary<int, BindingState?> WheelButtons { set; get; } = new Dictionary<int, BindingState?>();

        public BindingState? MouseScrollDown { set; get; }
        public BindingState? MouseScrollUp { set; get; }

        public DeltaThresholdBindingState? ClockwiseRotation { set; get; }
        public DeltaThresholdBindingState? CounterClockwiseRotation { set; get; }

        // Multi-wheel support: separate bindings for each wheel by index
        public Dictionary<int, DeltaThresholdBindingState?> MultiWheelClockwiseRotations { set; get; } = new Dictionary<int, DeltaThresholdBindingState?>();
        public Dictionary<int, DeltaThresholdBindingState?> MultiWheelCounterClockwiseRotations { set; get; } = new Dictionary<int, DeltaThresholdBindingState?>();

        public PipelinePosition Position => PipelinePosition.PostTransform;

        private readonly TabletReference tablet;
        private readonly uint? wheelSteps;
        private readonly double? halfWheelSteps;
        private readonly double? threeHalfWheelSteps;

        private uint? lastWheelPosition;
        private float currentWheelDelta;

        // Multi-wheel support: track delta for each wheel independently
        private readonly Dictionary<int, float> multiWheelDeltas = new Dictionary<int, float>();

        public event Action<IDeviceReport>? Emit;

        public void Consume(IDeviceReport report)
        {
            HandleBinding(report);
            Emit?.Invoke(report);
        }

        public void HandleBinding(IDeviceReport report)
        {
            if (report is IEraserReport eraserReport)
                _isEraser = eraserReport.Eraser;
            if (report is ITabletReport tabletReport)
                HandleTabletReport(tablet, tablet.Properties.Specifications.Pen, tabletReport);
            if (report is IAuxReport auxReport)
                HandleAuxiliaryReport(tablet, auxReport);
            if (report is IMouseReport mouseReport)
                HandleMouseReport(tablet, mouseReport);
            if (report is IWheelButtonReport wheelButtonReport)
                HandleWheelButtonReport(tablet, wheelButtonReport);
            if (report is IAbsoluteWheelReport absoluteWheelReport)
                HandleAbsoluteWheelReport(tablet, absoluteWheelReport);
            // Handle multi-wheel reports with separate bindings per wheel
            if (report is IMultiRelativeWheelReport multiWheelReport)
                HandleMultiRelativeWheelReport(tablet, multiWheelReport, multiWheelReport.WheelIndex, multiWheelReport.Delta);
            else if (report is IRelativeWheelReport relativeWheelReport)
                HandleRelativeWheelReport(tablet, relativeWheelReport, relativeWheelReport.Delta);
            if (report is OutOfRangeReport)
                HandleOutOfRangeReport(tablet, report);
        }

        private void HandleOutOfRangeReport(TabletReference tablet, IDeviceReport report)
        {
            for (var i = 0; i < PenButtons.Count; i++)
            {
                if (PenButtons.TryGetValue(i, out var binding))
                    binding?.Invoke(tablet, report, false);
            }
        }

        private void HandleTabletReport(TabletReference tablet, PenSpecifications pen, ITabletReport report)
        {
            float pressurePercent = (float)report.Pressure / (float)pen.MaxPressure * 100f;
            if (_isEraser)
                Eraser?.Invoke(tablet, report, pressurePercent);
            else
                Tip?.Invoke(tablet, report, pressurePercent);

            HandleBindingCollection(tablet, report, PenButtons, report.PenButtons);
        }

        private void HandleAuxiliaryReport(TabletReference tablet, IAuxReport report)
        {
            HandleBindingCollection(tablet, report, AuxButtons, report.AuxButtons);
        }

        private void HandleMouseReport(TabletReference tablet, IMouseReport report)
        {
            HandleBindingCollection(tablet, report, MouseButtons, report.MouseButtons);

            MouseScrollDown?.Invoke(tablet, report, report.Scroll.Y < 0);
            MouseScrollUp?.Invoke(tablet, report, report.Scroll.Y > 0);
        }

        private void HandleWheelButtonReport(TabletReference tablet, IWheelButtonReport report)
        {
            HandleBindingCollection(tablet, report, WheelButtons, report.WheelButtons);
        }

        private void HandleAbsoluteWheelReport(TabletReference tablet, IAbsoluteWheelReport report)
        {
            int? delta = ComputeWheelDelta(lastWheelPosition, report.Position);
            HandleRelativeWheelReport(tablet, report, delta);
            lastWheelPosition = report.Position;
        }

        private void HandleRelativeWheelReport(TabletReference tablet, IDeviceReport report, int? delta)
        {
            currentWheelDelta += delta ?? 0;

            ClockwiseRotation?.Invoke(tablet, report, ref currentWheelDelta);
            CounterClockwiseRotation?.Invoke(tablet, report, ref currentWheelDelta);

            // Some issues with keys staying pressed when holding on specific tablets
            // This will cause consistency issues on higher end machines
            ClockwiseRotation?.Invoke(tablet, report, false);
            CounterClockwiseRotation?.Invoke(tablet, report, false);
        }

        private void HandleMultiRelativeWheelReport(TabletReference tablet, IDeviceReport report, int wheelIndex, int? delta)
        {
            // Initialize delta tracking for this wheel if not present
            if (!multiWheelDeltas.ContainsKey(wheelIndex))
                multiWheelDeltas[wheelIndex] = 0;

            multiWheelDeltas[wheelIndex] += delta ?? 0;
            float wheelDelta = multiWheelDeltas[wheelIndex];

            // Try wheel-specific bindings first
            if (MultiWheelClockwiseRotations.TryGetValue(wheelIndex, out var cwBinding) && cwBinding != null)
            {
                cwBinding.Invoke(tablet, report, ref wheelDelta);
                multiWheelDeltas[wheelIndex] = wheelDelta;
                cwBinding.Invoke(tablet, report, false);
            }

            if (MultiWheelCounterClockwiseRotations.TryGetValue(wheelIndex, out var ccwBinding) && ccwBinding != null)
            {
                ccwBinding.Invoke(tablet, report, ref wheelDelta);
                multiWheelDeltas[wheelIndex] = wheelDelta;
                ccwBinding.Invoke(tablet, report, false);
            }

            // Fall back to the default wheel bindings if no wheel-specific bindings are set
            if (!MultiWheelClockwiseRotations.ContainsKey(wheelIndex) && !MultiWheelCounterClockwiseRotations.ContainsKey(wheelIndex))
            {
                HandleRelativeWheelReport(tablet, report, delta);
            }
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

        private static void HandleRangeBindingCollection(TabletReference tablet, IDeviceReport report, IDictionary<int, RangeBindingState?> bindings, float value)
        {
            for (int i = 0; i < bindings.Count; i++)
            {
                if (bindings.TryGetValue(i, out var binding))
                    binding?.Invoke(tablet, report, value);
            }
        }
    }
}
