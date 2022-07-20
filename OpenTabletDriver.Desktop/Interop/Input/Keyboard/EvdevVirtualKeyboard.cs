using System;
using System.Collections.Generic;
using System.Linq;
using OpenTabletDriver.Native.Linux;
using OpenTabletDriver.Native.Linux.Evdev;
using OpenTabletDriver.Platform.Keyboard;

namespace OpenTabletDriver.Desktop.Interop.Input.Keyboard
{
    public class EvdevVirtualKeyboard : IVirtualKeyboard, IDisposable
    {
        private readonly IKeysProvider _keysProvider;

        public unsafe EvdevVirtualKeyboard(IKeysProvider keysProvider)
        {
            _keysProvider = keysProvider;

            Device = new EvdevDevice("OpenTabletDriver Virtual Keyboard");

            var keyCodes = _keysProvider.EtoToNative.Values.Distinct()
                .Select(k => (EventCode)Convert.ToUInt32(k))
                .ToArray();
            Device.EnableTypeCodes(EventType.EV_KEY, keyCodes);

            var result = Device.Initialize();
            switch (result)
            {
                case ERRNO.NONE:
                    Log.Debug("Evdev", $"Successfully initialized virtual keyboard. (code {result})");
                    break;
                default:
                    Log.Write("Evdev", $"Failed to initialize virtual keyboard. (error code {result})", LogLevel.Error);
                    break;
            }
        }

        private EvdevDevice Device { get; }

        public IEnumerable<string> SupportedKeys => _keysProvider.EtoToNative.Keys;

        private void KeyEvent(string key, bool isPress)
        {
            var keyEventCode = (EventCode)_keysProvider.EtoToNative[key];

            Device.Write(EventType.EV_KEY, keyEventCode, isPress ? 1 : 0);
            Device.Sync();
        }

        public void Press(string key)
        {
            KeyEvent(key, true);
        }

        public void Release(string key)
        {
            KeyEvent(key, false);
        }

        public void Press(IEnumerable<string> keys)
        {
            foreach (var key in keys)
                KeyEvent(key, true);
        }

        public void Release(IEnumerable<string> keys)
        {
            foreach (var key in keys)
                KeyEvent(key, false);
        }

        public void Dispose()
        {
            Device?.Dispose();
        }
    }
}
