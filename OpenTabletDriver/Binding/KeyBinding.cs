using System;
using System.Linq;
using OpenTabletDriver.Interop;
using OpenTabletDriver.Interop.Input.Keyboard;
using OpenTabletDriver.Native;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Platform.Keyboard;

namespace OpenTabletDriver.Binding
{
    [PluginName("Key Binding")]
    public class KeyBinding : IBinding, IValidateBinding
    {
        private IVirtualKeyboard keyboard => Platform.KeyboardHandler;

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
            get => SystemInfo.CurrentPlatform switch
            {
                RuntimePlatform.Windows => WindowsVirtualKeyboard.EtoKeysymToVK.Keys.ToArray(),
                RuntimePlatform.Linux   => EvdevVirtualKeyboard.EtoKeysymToEventCode.Keys.ToArray(),
                RuntimePlatform.MacOS   => MacOSVirtualKeyboard.EtoKeysymToVK.Keys.ToArray(),
                _                       => null
            };
        } 
            

        public override string ToString() => BindingTools.GetShortBindingString(this);
    }
}