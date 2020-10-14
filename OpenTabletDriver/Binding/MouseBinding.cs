using System;
using System.Linq;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Binding
{
    [PluginName("Mouse Button Binding")]
    public class MouseBinding : IBinding, IValidateBinding
    {
        private IVirtualPointer pointer => Info.Driver.OutputMode.Pointer;
        
        public string Property { set; get; }
        
        public Action Press 
        {
            get 
            {
                if (Enum.TryParse<MouseButton>(Property, true, out var mouseButton))
                    return () => pointer.MouseDown(mouseButton);
                else
                    return null;
            }
        }

        public Action Release
        {
            get
            {
                if (Enum.TryParse<MouseButton>(Property, true, out var mouseButton))
                    return () => pointer.MouseUp(mouseButton);
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