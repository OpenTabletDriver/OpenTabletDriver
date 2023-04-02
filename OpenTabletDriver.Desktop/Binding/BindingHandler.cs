using System;
using System.Collections.Generic;
using System.Linq;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Output;
using OpenTabletDriver.Platform.Pointer;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Desktop.Binding
{
    public class BindingHandler : IPipelineElement<IDeviceReport>
    {
        private readonly InputDevice _device;
        private readonly IServiceProvider _serviceProvider;
        private bool _isEraser;
        private uint? _lastWheelPosition = null;

        public BindingHandler(IServiceProvider serviceProvider, InputDevice device, BindingSettings settings, IMouseButtonHandler? mouseButtonHandler = null)
        {
            _serviceProvider = serviceProvider;
            _device = device;

            var outputMode = _device.OutputMode!;

            // Force consume all reports from the last element
            var lastElement = outputMode.Elements?.LastOrDefault() ?? (IPipelineElement<IDeviceReport>)outputMode;
            lastElement.Emit += Consume;

            Tip = CreateBindingState<ThresholdBindingState>(settings.TipButton, device, mouseButtonHandler);

            if (Tip != null)
                Tip.ActivationThreshold = settings.TipActivationThreshold;

            Eraser = CreateBindingState<ThresholdBindingState>(settings.EraserButton, device, mouseButtonHandler);
            if (Eraser != null)
                Eraser.ActivationThreshold = settings.EraserActivationThreshold;

            PenButtons = CreateBindingStates(settings.PenButtons, device, mouseButtonHandler);
            AuxButtons = CreateBindingStates(settings.AuxButtons, device, mouseButtonHandler);
            MouseButtons = CreateBindingStates(settings.MouseButtons, device, mouseButtonHandler);

            MouseScrollDown = CreateBindingState<BindingState>(settings.MouseScrollDown, device, mouseButtonHandler);
            MouseScrollUp = CreateBindingState<BindingState>(settings.MouseScrollUp, device, mouseButtonHandler);

            WheelClockwise = CreateBindingState<BindingState>(settings.WheelClockwise, device, mouseButtonHandler);
            WheelCounterClockwise = CreateBindingState<BindingState>(settings.WheelCounterClockwise, device, mouseButtonHandler);
            WheelTouch = CreateBindingState<BindingState>(settings.WheelTouch, device, mouseButtonHandler);
        }

        private ThresholdBindingState? Tip { get; }
        private ThresholdBindingState? Eraser { get; }

        private Dictionary<int, BindingState?> PenButtons { get; }
        private Dictionary<int, BindingState?> AuxButtons { get; }
        private Dictionary<int, BindingState?> MouseButtons { get; }

        private BindingState? MouseScrollDown { get; }
        private BindingState? MouseScrollUp { get; }

        private BindingState? WheelClockwise { get; }
        private BindingState? WheelCounterClockwise { get; }
        private BindingState? WheelTouch { get; }

        public event Action<IDeviceReport>? Emit;

        public void Consume(IDeviceReport report)
        {
            Emit?.Invoke(report);
            HandleBinding(_device.OutputMode!.Tablet, report);
        }

        private void HandleBinding(InputDevice device, IDeviceReport report)
        {
            if (report is OutOfRangeReport oor)
            {
                ResetPenBindings(oor);
                return;
            }
            if (report is IEraserReport eraserReport)
                _isEraser = eraserReport.Eraser;
            if (report is ITabletReport tabletReport)
                HandleTabletReport(device.Configuration.Specifications.Pen!, tabletReport);
            if (report is IAuxReport auxReport)
                HandleAuxiliaryReport(auxReport);
            if(report is IAbsoluteWheelReport absoluteWheelReport)
                HandleAbsoluteWheelReport(absoluteWheelReport);
            if (report is IMouseReport mouseReport)
                HandleMouseReport(mouseReport);
        }

        private void ResetPenBindings(OutOfRangeReport oor)
        {
            foreach (var binding in PenButtons.Values)
            {
                binding?.Invoke(oor, false);
            }

            if (_isEraser)
                Eraser?.Invoke(oor, 0);
            else
                Tip?.Invoke(oor, 0);

            _isEraser = false;
        }

        private void HandleTabletReport(PenSpecifications pen, ITabletReport report)
        {
            var pressurePercent = report.Pressure / (float)pen.MaxPressure * 100f;
            if (_isEraser)
                Eraser?.Invoke(report, pressurePercent);
            else
                Tip?.Invoke(report, pressurePercent);

            HandleBindingCollection(report, PenButtons, report.PenButtons);
        }

        private void HandleAuxiliaryReport(IAuxReport report)
        {
            HandleBindingCollection(report, AuxButtons, report.AuxButtons);
        }

        private void HandleAbsoluteWheelReport(IAbsoluteWheelReport report)
        {
            var wheelSpec = _device.Configuration.Specifications.Wheel;
            if (wheelSpec == null)
                return;

            if (_lastWheelPosition == null || report.WheelPosition == null)
            {
                //First/last report
                WheelTouch?.Invoke(report, report.WheelPosition != null);
            }
            else
            {
                var movement =
                    ComputeShortCircleDistance(_lastWheelPosition.Value, report.WheelPosition.Value, wheelSpec.StepCount);

                //If we ever support Relative Wheels, everything below could possibly be turned into a virtual relative wheel.
                movement *= wheelSpec.IsClockwise ? 1 : -1;

                WheelClockwise?.Invoke(report, movement < 0);
                WheelCounterClockwise?.Invoke(report, movement > 0);
            }

            _lastWheelPosition = report.WheelPosition;
        }

        private long ComputeShortCircleDistance(uint from, uint to, uint steps)
        {
            var halfSteps = steps / 2;
            var treeHalf = halfSteps * 3;

            var movement = (((int)to - from + treeHalf) % steps) - halfSteps;
            return movement;
        }

        private void HandleMouseReport(IMouseReport report)
        {
            HandleBindingCollection(report, MouseButtons, report.MouseButtons);

            MouseScrollDown?.Invoke(report, report.Scroll.Y < 0);
            MouseScrollUp?.Invoke(report, report.Scroll.Y > 0);
        }

        /// <summary>
        /// Updates the button(binding) states for a collection of bindings based upon an array of buttons reported by a device
        /// </summary>
        /// <param name="report">The report from the device</param>
        /// <param name="bindings">The collection of bindings we are updating</param>
        /// <param name="newStates">New states of the updated bindings</param>
        /// <param name="offset">An offset into <paramref name="bindings"/> before <paramref name="newStates"/> applies</param>
        private static void HandleBindingCollection(IDeviceReport report, IDictionary<int, BindingState?>? bindings, IList<bool> newStates)
        {
            for (var i = 0; i < newStates.Count; i++)
            {
                if (bindings != null && bindings.TryGetValue(i, out var binding))
                    binding?.Invoke(report, newStates[i]);
            }
        }

        private T? CreateBindingState<T>(PluginSettings? settings, params object?[] args) where T : BindingState
        {
            if (settings == null)
                return null;

            var newArgs = args.TakeWhile(c => c != null)
                .Select(c => c!)
                .Append(settings);

            return _serviceProvider.CreateInstance<T>(newArgs.ToArray());
        }

        private Dictionary<int, BindingState?> CreateBindingStates(PluginSettingsCollection collection, params object?[] args)
        {
            var dict = new Dictionary<int, BindingState?>();

            for (var index = 0; index < collection.Count; index++)
            {
                var state = CreateBindingState<BindingState>(collection[index], args);

                if (!dict.TryAdd(index, state))
                    dict[index] = state;
            }

            return dict;
        }
    }
}
