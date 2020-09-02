using System;
using System.Linq;
using TabletDriverLib.Interop;
using TabletDriverPlugin;
using TabletDriverPlugin.Attributes;
using TabletDriverPlugin.Platform.Pointer;

namespace TabletDriverLib.Binding
{
    [PluginName("Mouse Button Binding")]
    public class MouseBinding : IBinding, IValidateBinding
    {
        public string Property { set; get; }
        
        public Action Press 
        {
            get 
            {
                IVirtualPointer mouseHandler = DriverState.OutputMode.Pointer;
                if (Enum.TryParse<MouseButton>(Property, true, out var mouseButton))
                    return () => mouseHandler.MouseDown(mouseButton);
                else
                    return null;
            }
        }

        public Action Release
        {
            get
            {
                IVirtualPointer mouseHandler = DriverState.OutputMode.Pointer;
                if (Enum.TryParse<MouseButton>(Property, true, out var mouseButton))
                    return () => mouseHandler.MouseUp(mouseButton);
                else
                    return null;
            }
        }

        public string[] ValidProperties
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

        public override string ToString() => BindingTools.GetShortBindingString(this);
    }
}