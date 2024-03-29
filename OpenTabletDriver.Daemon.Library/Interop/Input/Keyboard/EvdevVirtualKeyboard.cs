using System;
using System.Collections.Generic;
using System.Linq;
using OpenTabletDriver.Native.Linux;
using OpenTabletDriver.Native.Linux.Evdev;
using OpenTabletDriver.Platform.Keyboard;

namespace OpenTabletDriver.Daemon.Interop.Input.Keyboard
{
    public class EvdevVirtualKeyboard : IVirtualKeyboard, IDisposable
    {
        private readonly IKeyMapper _keyMapper;

        public unsafe EvdevVirtualKeyboard(IKeyMapper keyMapper)
        {
            _keyMapper = keyMapper;

            Device = new EvdevDevice("OpenTabletDriver Virtual Keyboard");

            var keyCodes = _keyMapper.GetBindableKeys()
                .Select(k => (EventCode)_keyMapper[k]);

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

        public IEnumerable<BindableKey> SupportedKeys => _keyMapper.GetBindableKeys();

        private void KeyEvent(BindableKey key, bool isPress)
        {
            var keyEventCode = (EventCode)_keyMapper[key];

            Device.Write(EventType.EV_KEY, keyEventCode, isPress ? 1 : 0);
            Device.Sync();
        }

        public void Press(BindableKey key)
        {
            KeyEvent(key, true);
        }

        public void Release(BindableKey key)
        {
            KeyEvent(key, false);
        }

        public void Press(IEnumerable<BindableKey> keys)
        {
            foreach (var key in keys)
                KeyEvent(key, true);
        }

        public void Release(IEnumerable<BindableKey> keys)
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
