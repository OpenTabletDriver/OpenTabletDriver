using System;
using System.Linq;
using NativeLib;
using TabletDriverLib.Interop;
using TabletDriverLib.Interop.Input.Keyboard;
using TabletDriverPlugin;
using TabletDriverPlugin.Attributes;
using TabletDriverPlugin.Platform.Keyboard;

namespace TabletDriverLib.Binding
{
    [PluginName("Key Binding")]
    public class KeyBinding : IBinding, IValidateBinding
    {
        public string Property { set; get; }

        public Action Press
        {
            get
            {
                IVirtualKeyboard keyboardHandler = Platform.KeyboardHandler;
                return () => keyboardHandler.Press(Property);
            }
        }

        public Action Release
        {
            get
            {
                IVirtualKeyboard keyboardHandler = Platform.KeyboardHandler;
                return () => keyboardHandler.Release(Property);
            }
        }

        public string[] ValidProperties => 
            SystemInfo.CurrentPlatform switch
            {
                RuntimePlatform.Windows => WindowsVirtualKeyboard.EtoKeysymToVK.Keys.ToArray(),
                RuntimePlatform.Linux   => EvdevVirtualKeyboard.EtoKeysymToEventCode.Keys.ToArray(),
                RuntimePlatform.MacOS   => MacOSVirtualKeyboard.EtoKeysymToVK.Keys.ToArray(),
                _                       => null
            };

        public override string ToString() => BindingTools.GetShortBindingString(this);
    }
}