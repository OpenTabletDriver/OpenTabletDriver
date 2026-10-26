using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Binding
{
    [PluginIgnore]
    public class BindingHandler : IPositionedPipelineElement
    {
        public BindingHandler(TabletReference tablet)
        {
            this._tablet = tablet;

            int wheelIndex = 0;
            foreach (var wheel in tablet.Properties.Specifications.Wheels ?? [])
                Wheels.Add(wheelIndex++, new WheelBindings(wheel));
        }

        public ThresholdBindingState? Tip { set; get; }
        public ThresholdBindingState? Eraser { set; get; }
        private bool _isEraser;

        public Dictionary<int, BindingState?> PenButtons { set; get; } = new Dictionary<int, BindingState?>();
        public Dictionary<int, BindingState?> AuxButtons { set; get; } = new Dictionary<int, BindingState?>();
        public Dictionary<int, BindingState?> MouseButtons { set; get; } = new Dictionary<int, BindingState?>();

        public BindingState? MouseScrollDown { set; get; }
        public BindingState? MouseScrollUp { set; get; }

        public Dictionary<int, WheelBindings> Wheels { get; } = new Dictionary<int, WheelBindings>();

        public PipelinePosition Position => PipelinePosition.PostTransform;

        private readonly TabletReference _tablet;

        public event RefAction<DeviceReport>? Emit;

        public void Consume(scoped ref DeviceReport report)
        {
            handleBinding(in report);

            Emit?.Invoke(ref report);
        }

        private void handleBinding(scoped in DeviceReport report)
        {
            if (report.Eraser.HasValue)
                _isEraser = report.Eraser.Value;
            if (report.PenPosition.HasValue)
                handleTabletReport(_tablet, _tablet.Properties.Specifications.Pen, in report);
            if (report.AuxButtons != null)
                handleAuxiliaryReport(_tablet, in report);
            if (report.MouseButtons != null && report.MouseScroll.HasValue)
                handleMouseReport(_tablet, in report);
            if (report.WheelButtons != null)
                handleWheelButtonReport(_tablet, in report);
            if (report.WheelPositions != null)
                handleAbsoluteWheelReport(_tablet, in report);
            if (report.WheelDeltas != null)
                handleRelativeWheelReport(_tablet, in report);
            if (report.OutOfRange)
                handleOutOfRangeReport(_tablet, in report);
        }

        private readonly HashSet<int> _triedRelativeWheels = [];

        private void handleRelativeWheelReport(TabletReference tabletReference, scoped in DeviceReport report)
        {
            Debug.Assert(report.WheelDeltas != null);
            for (int i = 0; i < report.WheelDeltas.Length; i++)
            {
                int reportDelta = report.WheelDeltas.Span[i];

                if (Wheels.TryGetValue(i, out var wheelBinding))
                    wheelBinding.HandleRelativeWheel(tabletReference, report, reportDelta);
                else if (reportDelta != 0 && _triedRelativeWheels.Add(i))
                {
                    Log.Write(nameof(BindingHandler),
                        $"Tablet '{_tablet.Properties.Name}' is missing wheel declarations for wheel '{i}' to handle its wheel bindings",
                        LogLevel.Warning);
                }
            }
        }

        private readonly HashSet<int> _triedAbsoluteWheels = [];

        private void handleAbsoluteWheelReport(TabletReference tabletReference, scoped in DeviceReport report)
        {
            Debug.Assert(report.WheelPositions != null);

            for (int i = 0; i < report.WheelPositions.Length; i++)
            {
                uint? reportPosition = report.WheelPositions.Span[i];

                if (Wheels.TryGetValue(i, out var wheelBinding))
                    wheelBinding.HandleAbsoluteWheel(tabletReference, in report, reportPosition);
                else if (reportPosition != null && _triedAbsoluteWheels.Add(i))
                {
                    Log.Write(nameof(BindingHandler),
                        $"Tablet '{_tablet.Properties.Name}' is missing wheel declarations for wheel '{i}' to handle its wheel bindings",
                        LogLevel.Warning);
                }
            }
        }

        private readonly HashSet<int> _triedWheelButtons = [];

        private void handleWheelButtonReport(TabletReference tabletReference, scoped in DeviceReport report)
        {
            Debug.Assert(report.WheelButtons != null);
            for (int i = 0; i < report.WheelButtons.Length; i++)
            {
                if (Wheels.TryGetValue(i, out var wheelBinding))
                {
                    handleBindingCollection(tabletReference, report, wheelBinding.WheelButtons, report.WheelButtons.Span[i].Span);
                }
                else if (_triedWheelButtons.Add(i))
                {
                    Log.Write(nameof(BindingHandler),
                        $"Tablet '{_tablet.Properties.Name}' is missing wheel declarations for wheel '{i}' to handle its wheel button bindings",
                        LogLevel.Warning);
                }
            }
        }

        private void handleOutOfRangeReport(TabletReference tablet, scoped in DeviceReport report)
        {
            for (var i = 0; i < PenButtons.Count; i++)
            {
                if (PenButtons.TryGetValue(i, out var binding))
                    binding?.Invoke(tablet, in report, false);
            }
        }

        private void handleTabletReport(TabletReference tablet, PenSpecifications pen, scoped in DeviceReport report)
        {
            Debug.Assert(report.PenPressure.HasValue);
            float pressurePercent = report.PenPressure.Value / (float)pen.MaxPressure * 100f;
            if (_isEraser)
                Eraser?.Invoke(tablet, in report, pressurePercent);
            else
                Tip?.Invoke(tablet, in report, pressurePercent);

            if (report.PenButtons != null)
                handleBindingCollection(tablet, in report, PenButtons, report.PenButtons.Span);
        }

        private void handleAuxiliaryReport(TabletReference tablet, scoped in DeviceReport report)
        {
            Debug.Assert(report.AuxButtons != null);
            handleBindingCollection(tablet, in report, AuxButtons, report.AuxButtons.Span);
        }

        private void handleMouseReport(TabletReference tablet, scoped in DeviceReport report)
        {
            if (report.MouseButtons != null)
                handleBindingCollection(tablet, in report, MouseButtons, report.MouseButtons.Span);

            if (report.MouseScroll.HasValue)
            {
                var scrollValue = report.MouseScroll.Value.Y;
                MouseScrollDown?.Invoke(tablet, in report, scrollValue < 0);
                MouseScrollUp?.Invoke(tablet, in report, scrollValue > 0);
            }
        }

        private static void handleBindingCollection(TabletReference tablet, scoped in DeviceReport report, Dictionary<int, BindingState?> bindings, Span<bool> newStates)
        {
            for (int i = 0; i < newStates.Length; i++)
            {
                if (bindings.TryGetValue(i, out var binding))
                    binding?.Invoke(tablet, in report, newStates[i]);
            }
        }
    }
}
