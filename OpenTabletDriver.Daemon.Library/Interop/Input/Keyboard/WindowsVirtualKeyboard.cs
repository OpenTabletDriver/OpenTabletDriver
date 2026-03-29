using System;
using System.Collections.Generic;
using OpenTabletDriver.Native.Windows.Input;
using OpenTabletDriver.Platform.Keyboard;
using static OpenTabletDriver.Native.Windows.WindowsAPI;

namespace OpenTabletDriver.Daemon.Library.Interop.Input.Keyboard
{
    public class WindowsVirtualKeyboard : IVirtualKeyboard
    {
        private readonly IKeyMapper _keyMapper;

        public WindowsVirtualKeyboard(IKeyMapper keysProvider)
        {
            _keyMapper = keysProvider;
        }

        private void KeyEvent(BindableKey key, bool isPress)
        {
            var vk = (VirtualKey)_keyMapper[key];
            var input = new INPUT
            {
                type = INPUT_TYPE.KEYBD_INPUT,
                U = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = (short)vk,
                        wScan = 0,
                        dwFlags = isPress ? KEYEVENTF.KEYDOWN : KEYEVENTF.KEYUP,
                        time = 0,
                        dwExtraInfo = UIntPtr.Zero
                    }
                }
            };

            var inputs = new INPUT[] { input };
            _ = SendInput((uint)inputs.Length, inputs, INPUT.Size);
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

        public IEnumerable<BindableKey> SupportedKeys => _keyMapper.GetBindableKeys();
    }
}
