using System;
using TabletDriverLib.Interop;
using TabletDriverPlugin;
using TabletDriverPlugin.Attributes;
using TabletDriverPlugin.Platform.Pointer;

namespace TabletDriverLib.Binding
{
    [PluginName("Mouse Button Binding")]
    public class MouseBinding : IBinding
    {
        public string Name
        {
            get
            {
                return nameof(MouseBinding) + ": " + Property;
            }
        }

        public Action Press 
        {
            get 
            {
                ICursorHandler cursorHandler = Platform.CursorHandler;
                if (Enum.TryParse<MouseButton>(Property, true, out var mouseButton))
                    return () => cursorHandler.MouseDown(mouseButton);
                else
                    return null;
            }
        }

        public Action Release
        {
            get
            {
                ICursorHandler cursorHandler = Platform.CursorHandler;
                if (Enum.TryParse<MouseButton>(Property, true, out var mouseButton))
                    return () => cursorHandler.MouseUp(mouseButton);
                else
                    return null;
            }
        }

        public string Property { set; get; }

        public override string ToString() => Name;
    }
}