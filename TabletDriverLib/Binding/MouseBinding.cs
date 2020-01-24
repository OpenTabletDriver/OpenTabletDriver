using System;
using TabletDriverLib.Interop;
using TabletDriverLib.Interop.Cursor;
using TabletDriverPlugin;

namespace TabletDriverLib.Binding
{
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