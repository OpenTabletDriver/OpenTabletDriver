using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Plugin.Platform.Keyboard;

namespace OpenTabletDriver.Tests.Interop
{
    [TestClass]
    public class KeyboardTest
    {
        [TestInitialize]
        public void Initialize()
        {
            Keyboard = DesktopInterop.VirtualKeyboard;
        }

        public IVirtualKeyboard Keyboard { set; get; }

        [DataTestMethod]
        [DataRow("Up"), DataRow("Down"), DataRow("Left"), DataRow("Right")]
        [DataRow("Control"), DataRow("Alt"), DataRow("Application")]
        public void TestSingleKey(string key)
        {
            Keyboard.Press(key);
            Thread.Sleep(100);
            Keyboard.Release(key);
            Thread.Sleep(100);
        }

        [DataTestMethod]
        [DataRow("Control+Alt+Up"), DataRow("Control+Alt+Down")]
        [DataRow("Shift+Up"), DataRow("Shift+Down")]
        public void TestMultiKey(string keysStr)
        {
            var keys = keysStr.Split('+', System.StringSplitOptions.TrimEntries);

            Keyboard.Press(keys);
            Thread.Sleep(100);
            Keyboard.Release(keys);
            Thread.Sleep(100);
        }
    }
}