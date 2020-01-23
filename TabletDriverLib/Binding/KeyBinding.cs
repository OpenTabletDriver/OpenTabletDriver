using System;
using TabletDriverLib.Interop;
using TabletDriverLib.Interop.Keyboard;
using TabletDriverPlugin;

namespace TabletDriverLib.Binding
{
    public class KeyBinding : IBinding
    {
        public string Name
        {
            get
            {
                return nameof(KeyBinding) + ": " + Property;
            }
        }

        public string Property { set; get; }

        public Action Press
        {
            get
            {
                IKeyboardHandler keyboardHandler = Platform.KeyboardHandler;
                return () => keyboardHandler.Press(Property);
            }
        }

        public Action Release
        {
            get
            {
                IKeyboardHandler keyboardHandler = Platform.KeyboardHandler;
                return () => keyboardHandler.Release(Property);
            }
        }

        public override string ToString() => Name;
    }
}