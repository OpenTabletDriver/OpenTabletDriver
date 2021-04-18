using System.Linq;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Desktop.Interop.Input.Keyboard;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.DependencyInjection;
using OpenTabletDriver.Plugin.Platform.Keyboard;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Binding
{
    [PluginName(PLUGIN_NAME)]
    public class KeyBinding : IBinding
    {
        private const string PLUGIN_NAME = "Key Binding";

        [Resolved]
        public IVirtualKeyboard Keyboard { set; get; }

        [Property("Keys"), PropertyValidated(nameof(ValidKeys))]
        public string Keys { set; get; }

        public void Press(IDeviceReport report)
        {
            Keyboard.Press(Keys);
        }

        public void Release(IDeviceReport report)
        {
            Keyboard.Release(Keys);
        }

        public static string[] ValidKeys
        {
            get => DesktopInterop.CurrentPlatform switch
            {
                PluginPlatform.Windows => WindowsVirtualKeyboard.EtoKeysymToVK.Keys.ToArray(),
                PluginPlatform.Linux   => EvdevVirtualKeyboard.EtoKeysymToEventCode.Keys.ToArray(),
                PluginPlatform.MacOS   => MacOSVirtualKeyboard.EtoKeysymToVK.Keys.ToArray(),
                _                      => null
            };
        }

        public override string ToString() => $"{PLUGIN_NAME}: {Keys}";
    }
}
