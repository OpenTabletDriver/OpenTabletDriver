using System;
using System.Linq;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Binding
{
    [PluginName(PLUGIN_NAME)]
    public class MouseBinding : IBinding
    {
        private const string PLUGIN_NAME = "Mouse Button Binding";

        private IVirtualMouse pointer => Info.Driver.OutputMode switch
        {
            IPointerProvider<IRelativePointer> outputMode => outputMode.Pointer as IVirtualMouse,
            IPointerProvider<IAbsolutePointer> outputMode => outputMode.Pointer as IVirtualMouse,
            _ => null
        };

        [Property("Button"), PropertyValidated(nameof(ValidButtons))]
        public string Button { set; get; }

        public void Press(IDeviceReport report)
        {
            if (Enum.TryParse<MouseButton>(Button, true, out var mouseButton))
                pointer?.MouseDown(mouseButton);
        }

        public void Release(IDeviceReport report)
        {
            if (Enum.TryParse<MouseButton>(Button, true, out var mouseButton))
                pointer?.MouseUp(mouseButton);
        }

        public static string[] ValidButtons
        {
            get
            {
                var items = Enum.GetValues(typeof(MouseButton));
                var properties = new MouseButton[items.Length];
                items.CopyTo(properties, 0);
                var converted = from item in properties
                    select Enum.GetName(typeof(MouseButton), item);
                return converted.ToArray();
            }
        }

        public override string ToString() => $"{PLUGIN_NAME}: {Button}";
    }
}