using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OpenTabletDriver.Attributes;
using OpenTabletDriver.Output;
using OpenTabletDriver.Platform.Pointer;
using OpenTabletDriver.Tablet;
using OpenTabletDriver.Daemon.Contracts.Persistence;

namespace OpenTabletDriver.Daemon.Binding
{
    [PluginIgnore]
    public class BindingHandler : IDevicePipelineElement
    {
        private readonly InputDevice _device;
        private readonly IServiceProvider _serviceProvider;
        private bool _isEraser;

        public BindingHandler(IServiceProvider serviceProvider, InputDevice device, OutputMode outputMode, BindingSettings settings, IMouseButtonHandler? mouseButtonHandler = null)
        {
            _serviceProvider = serviceProvider;
            _device = device;

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
        }

        private ThresholdBindingState? Tip { get; }
        private ThresholdBindingState? Eraser { get; }

        private Dictionary<int, BindingState?> PenButtons { get; }
        private Dictionary<int, BindingState?> AuxButtons { get; }
        private Dictionary<int, BindingState?> MouseButtons { get; }

        private BindingState? MouseScrollDown { get; }
        private BindingState? MouseScrollUp { get; }

        public PipelinePosition Position => PipelinePosition.PostTransform;

        public event Action<IDeviceReport>? Emit;

        public void Consume(IDeviceReport report)
        {
            HandleBinding(_device.OutputMode!.Tablet, report);
            Emit?.Invoke(report);
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

        private void HandleMouseReport(IMouseReport report)
        {
            HandleBindingCollection(report, MouseButtons, report.MouseButtons);

            MouseScrollDown?.Invoke(report, report.Scroll.Y < 0);
            MouseScrollUp?.Invoke(report, report.Scroll.Y > 0);
        }

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

        private Dictionary<int, BindingState?> CreateBindingStates(Collection<PluginSettings?> collection, params object?[] args)
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
