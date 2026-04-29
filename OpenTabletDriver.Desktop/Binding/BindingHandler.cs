using System;
using System.Collections.Generic;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Wheel;

namespace OpenTabletDriver.Desktop.Binding
{
    [PluginIgnore]
    public class BindingHandler : IPositionedPipelineElement<IDeviceReport>
    {
        public BindingHandler(TabletReference tablet)
        {
            this.tablet = tablet;

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

        private readonly TabletReference tablet;

        public event Action<IDeviceReport?>? Emit;

        public void Consume(IDeviceReport? report)
        {
            if (report != null)
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
            if (report is IRelativeWheelReport relativeWheelReport)
                HandleRelativeWheelReport(tablet, relativeWheelReport);
            if (report is OutOfRangeReport)
                HandleOutOfRangeReport(tablet, report);
        }

        private readonly HashSet<int> _triedRelativeWheels = [];

        private void HandleRelativeWheelReport(TabletReference tabletReference, IRelativeWheelReport relativeWheelReport)
        {
            for (int i = 0; i < relativeWheelReport.AnalogDeltas.Length; i++)
            {
                int reportDelta = relativeWheelReport.AnalogDeltas[i];

                if (Wheels.TryGetValue(i, out var wheelBinding))
                    wheelBinding.HandleRelativeWheel(tabletReference, relativeWheelReport, reportDelta);
                else if (reportDelta != 0 && _triedRelativeWheels.Add(i))
                {
                    Log.Write(nameof(BindingHandler),
                        $"Tablet '{tablet.Properties.Name}' is missing wheel declarations for wheel '{i}' to handle its wheel bindings",
                        LogLevel.Warning);
                }
            }
        }

        private readonly HashSet<int> _triedAbsoluteWheels = [];

        private void HandleAbsoluteWheelReport(TabletReference tabletReference, IAbsoluteWheelReport absoluteWheelReport)
        {
            for (int i = 0; i < absoluteWheelReport.AnalogPositions.Length; i++)
            {
                uint? reportPosition = absoluteWheelReport.AnalogPositions[i];

                if (Wheels.TryGetValue(i, out var wheelBinding))
                    wheelBinding.HandleAbsoluteWheel(tabletReference, absoluteWheelReport, reportPosition);
                else if (reportPosition != null && _triedAbsoluteWheels.Add(i))
                {
                    Log.Write(nameof(BindingHandler),
                        $"Tablet '{tablet.Properties.Name}' is missing wheel declarations for wheel '{i}' to handle its wheel bindings",
                        LogLevel.Warning);
                }
            }
        }

        private readonly HashSet<int> _triedWheelButtons = [];

        private void HandleWheelButtonReport(TabletReference tabletReference, IWheelButtonReport wheelButtonReport)
        {
            for (int i = 0; i < wheelButtonReport.WheelButtons.Length; i++)
            {
                if (Wheels.TryGetValue(i, out var wheelBinding))
                {
                    bool[] wheelButton = wheelButtonReport.WheelButtons[i];
                    HandleBindingCollection(tabletReference, wheelButtonReport, wheelBinding.WheelButtons, wheelButton);
                }
                else if (_triedWheelButtons.Add(i))
                {
                    Log.Write(nameof(BindingHandler),
                        $"Tablet '{tablet.Properties.Name}' is missing wheel declarations for wheel '{i}' to handle its wheel button bindings",
                        LogLevel.Warning);
                }
            }
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

        private static void HandleBindingCollection(TabletReference tablet, IDeviceReport report, Dictionary<int, BindingState?> bindings, bool[] newStates)
        {
            for (int i = 0; i < newStates.Length; i++)
            {
                if (bindings.TryGetValue(i, out var binding))
                    binding?.Invoke(tablet, report, newStates[i]);
            }
        }
    }
}
