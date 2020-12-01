using System;
using System.Linq;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Desktop.Interop.Input.Keyboard;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Platform.Keyboard;

namespace OpenTabletDriver.Desktop.Binding
{
    [PluginName("Key Binding")]
    public class KeyBinding : IBinding, IValidateBinding
    {
        private IVirtualKeyboard keyboard => SystemInterop.KeyboardHandler;

        public string Property { set; get; }

        public Action Press
        {
            get => () => keyboard.Press(Property);
        }

        public Action Release
        {
            get => () => keyboard.Release(Property);
        }

        public string[] ValidProperties
        {
            get => SystemInterop.CurrentPlatform switch
            {
                PluginPlatform.Windows => WindowsVirtualKeyboard.EtoKeysymToVK.Keys.ToArray(),
                PluginPlatform.Linux   => EvdevVirtualKeyboard.EtoKeysymToEventCode.Keys.ToArray(),
                PluginPlatform.MacOS   => MacOSVirtualKeyboard.EtoKeysymToVK.Keys.ToArray(),
                _                       => null
            };
        } 
            

        public override string ToString() => BindingTools.GetShortBindingString(this);
    }
}
