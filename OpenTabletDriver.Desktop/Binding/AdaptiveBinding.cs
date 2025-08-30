using System;
using System.Collections.Generic;
using System.Linq;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.DependencyInjection;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Binding
{
    [PluginName(PluginName)]
    public class AdaptiveBinding : IStateBinding
    {
        private const string PluginName = "Adaptive Binding";

        [Resolved] public IPenActionHandler PenActionHandler { set; get; }

        [Resolved] public IMouseButtonHandler MouseButtonHandler { set; get; }

        // ReSharper disable once UnusedMember.Global
        public AdaptiveBinding()
        {
        }

        public AdaptiveBinding(PenAction action)
        {
            Binding = ActionToString(action);
        }

        public static string[] ButtonNames => ValidButtons.Keys.ToArray();

        private string _binding = string.Empty;

        [Property(nameof(Binding)), PropertyValidated(nameof(ButtonNames))]
        public string Binding
        {
            get => _binding;
            set
            {
                if (!ValidButtons.TryGetValue(value, out var button))
                    throw new ArgumentException("Invalid button name", value);

                _action = button;
                _binding = value;
            }
        }

        public void Press(TabletReference tablet, IDeviceReport report)
        {
            if (PenActionHandler != null)
                PenActionHandler.Activate(_action);
            else if (MouseButtonHandler != null)
            {
                var button = ActionToButton(_action);
                if (button != null)
                    MouseButtonHandler.MouseDown(button.Value);
            }
        }

        public void Release(TabletReference tablet, IDeviceReport report)
        {
            if (PenActionHandler != null)
                PenActionHandler.Deactivate(_action);
            else if (MouseButtonHandler != null)
            {
                var button = ActionToButton(_action);
                if (button != null)
                    MouseButtonHandler.MouseUp(button.Value);
            }
        }

        public override string ToString() => $"{PluginName}: {Binding}";

        private PenAction _action;

        private static MouseButton? ActionToButton(PenAction action)
        {
            return action switch
            {
                PenAction.Tip => MouseButton.Left,
                PenAction.Eraser => MouseButton.Left,
                PenAction.BarrelButton1 => MouseButton.Right,
                PenAction.BarrelButton2 => MouseButton.Middle,
                _ => null
            };

        }

        private static readonly Dictionary<string, PenAction> ValidButtons = new()
        {
            { "Tip", PenAction.Tip },
            { "Eraser", PenAction.Eraser },
            { "Button 1", PenAction.BarrelButton1 },
            { "Button 2", PenAction.BarrelButton2 },
        };

        private static string ActionToString(PenAction button) =>
            ValidButtons.Where(x => x.Value == button)
                .Select(x => x.Key)
                .FirstOrDefault();
    }
}
