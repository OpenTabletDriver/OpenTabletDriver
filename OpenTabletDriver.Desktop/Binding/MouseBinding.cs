using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Binding
{
    [PluginName(PLUGIN_NAME)]
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    public class MouseBinding(IMouseButtonHandler mouseButtonHandler) : IStateBinding
    {
        private const string PLUGIN_NAME = "Mouse Button Binding";

        [Property("Button"), PropertyValidated(nameof(ValidButtons))]
        public string? Button { set; get; }

        public void Press(TabletReference tablet, IDeviceReport report)
        {
            if (Enum.TryParse<MouseButton>(Button, true, out var mouseButton))
                mouseButtonHandler.MouseDown(mouseButton);
        }

        public void Release(TabletReference tablet, IDeviceReport report)
        {
            if (Enum.TryParse<MouseButton>(Button, true, out var mouseButton))
                mouseButtonHandler.MouseUp(mouseButton);
        }

        private static IEnumerable<string>? validButtons;
        public static IEnumerable<string> ValidButtons =>
            validButtons ??= Enum.GetValues<MouseButton>().Select(Enum.GetName)!;

        public override string ToString() => $"{PLUGIN_NAME}: {Button}";
    }
}
