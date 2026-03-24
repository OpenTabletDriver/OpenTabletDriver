using System;
using System.Collections.Generic;
using System.Linq;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.DependencyInjection;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Platform.Keyboard;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Binding
{
    [PluginName(PLUGIN_NAME)]
    public class KeyMouseBinding : IStateBinding
    {
        private const string PLUGIN_NAME = "Key + Mouse Binding";
        private const char KEYS_SPLITTER = '+';

        private string[] keys = Array.Empty<string>();
        private MouseButton? mouseButton;

        [Resolved]
        public IVirtualKeyboard Keyboard { set; get; }

        [Resolved]
        public IMouseButtonHandler Pointer { set; get; }

        [Property("Keys")]
        public string Keys
        {
            set
            {
                this.keysString = value;
                this.keys = ParseKeys(value);
            }
            get => this.keysString;
        }

        private string keysString;

        [Property("Button"), PropertyValidated(nameof(ValidButtons))]
        public string Button
        {
            set
            {
                this.buttonString = value;
                this.mouseButton = Enum.TryParse<MouseButton>(value, true, out var parsed) ? parsed : null;
            }
            get => this.buttonString;
        }

        private string buttonString;

        public void Press(TabletReference tablet, IDeviceReport report)
        {
            if (keys.Length > 0)
                Keyboard.Press(keys);
            if (mouseButton.HasValue)
                Pointer?.MouseDown(mouseButton.Value);
        }

        public void Release(TabletReference tablet, IDeviceReport report)
        {
            if (mouseButton.HasValue)
                Pointer?.MouseUp(mouseButton.Value);
            if (keys.Length > 0)
                Keyboard.Release(keys);
        }

        private string[] ParseKeys(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return Array.Empty<string>();
            var newKeys = str.Split(KEYS_SPLITTER, StringSplitOptions.TrimEntries);
            return newKeys.All(k => Keyboard.SupportedKeys.Contains(k)) ? newKeys : Array.Empty<string>();
        }

        private static IEnumerable<string> validButtons;
        public static IEnumerable<string> ValidButtons
        {
            get => validButtons ??= Enum.GetValues<MouseButton>().Select(Enum.GetName);
        }

        public override string ToString() => $"{PLUGIN_NAME}: {Keys} + {Button}";
    }
}
