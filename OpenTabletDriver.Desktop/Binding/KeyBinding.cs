using System;
using System.Linq;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Desktop.Interop.Input.Keyboard;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.DependencyInjection;
using OpenTabletDriver.Plugin.Platform.Keyboard;

namespace OpenTabletDriver.Desktop.Binding
{
    [PluginName("Key Binding")]
    public class KeyBinding : IBinding, IValidateBinding
    {
        [Resolved]
        public IVirtualKeyboard Keyboard { set; get; }

        [Property("Property")]
        public string Property { set; get; }

        public Action Press
        {
            get => () => Keyboard.Press(Property);
        }

        public Action Release
        {
            get => () => Keyboard.Release(Property);
        }

        public string[] ValidProperties
        {
            get => DesktopInterop.CurrentPlatform switch
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
