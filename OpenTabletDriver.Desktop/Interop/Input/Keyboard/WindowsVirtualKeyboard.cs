using System;
using System.Collections.Generic;
using OpenTabletDriver.Native.Windows;
using OpenTabletDriver.Native.Windows.Input;
using OpenTabletDriver.Platform.Keyboard;

namespace OpenTabletDriver.Desktop.Interop.Input.Keyboard
{
    using static Windows;

    public class WindowsVirtualKeyboard : IVirtualKeyboard
    {
        private readonly IKeysProvider _keysProvider;

        public WindowsVirtualKeyboard(IKeysProvider keysProvider)
        {
            _keysProvider = keysProvider;
        }

        private void KeyEvent(string key, bool isPress)
        {
            var vk = (VirtualKey)_keysProvider.EtoToNative[key];
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
            SendInput((uint)inputs.Length, inputs, INPUT.Size);
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

        public IEnumerable<string> SupportedKeys => _keysProvider.EtoToNative.Keys;
    }
}
