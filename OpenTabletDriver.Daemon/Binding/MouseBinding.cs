using System;
using System.Collections.Generic;
using System.Linq;
using OpenTabletDriver.Attributes;
using OpenTabletDriver.Platform.Pointer;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Daemon.Binding
{
    [PluginName(PLUGIN_NAME)]
    public class MouseBinding : IStateBinding
    {
        private readonly IMouseButtonHandler _pointer;

        public MouseBinding(IMouseButtonHandler pointer, ISettingsProvider settingsProvider)
        {
            _pointer = pointer;

            settingsProvider.Inject(this);
        }

        private const string PLUGIN_NAME = "Mouse Button Binding";

        [Setting("Button"), MemberValidated(nameof(ValidButtons))]
        public string Button { set; get; } = string.Empty;

        public void Press(IDeviceReport report)
        {
            if (Enum.TryParse<MouseButton>(Button, true, out var mouseButton))
                _pointer?.MouseDown(mouseButton);
        }

        public void Release(IDeviceReport report)
        {
            if (Enum.TryParse<MouseButton>(Button, true, out var mouseButton))
                _pointer?.MouseUp(mouseButton);
        }

        public static IEnumerable<string> ValidButtons { get; } = Enum.GetValues(typeof(MouseButton)).Cast<MouseButton>().Select(Enum.GetName)!;

        public override string ToString() => $"{PLUGIN_NAME}: {Button}";
    }
}
