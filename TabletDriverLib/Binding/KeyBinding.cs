using System;
using System.Linq;
using NativeLib;
using TabletDriverLib.Interop;
using TabletDriverLib.Interop.Keyboard;
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

        public string[] ValidProperties => 
            SystemInfo.CurrentPlatform switch
            {
                RuntimePlatform.Windows => WindowsKeyboardHandler.EtoKeysymToVK.Keys.ToArray(),
                RuntimePlatform.Linux   => EvdevKeyboardHandler.EtoKeysymToEventCode.Keys.ToArray(),
                RuntimePlatform.MacOS   => MacOSKeyboardHandler.EtoKeysymToVK.Keys.ToArray(),
                _                       => null
            };

        public override string ToString() => BindingTools.GetShortBindingString(this);
    }
}