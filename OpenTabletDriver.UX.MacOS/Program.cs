using System;
using Eto.Forms;
using OpenTabletDriver.Native.OSX.ApplicationServices;
using OpenTabletDriver.Native.OSX.IOkit;

namespace OpenTabletDriver.UX.MacOS
{
    class Program
    {
        private static bool checkAccessibility()
        {
            return Environment.OSVersion.Version < new Version(10, 4) ||
                ApplicationServices.AXIsProcessTrusted();
        }

        private static bool checkInpuMonitoring()
        {
            return Environment.OSVersion.Version < new Version(10, 15) ||
                IOKit.IOHIDCheckAccess(IOHIDRequestType.kIOHIDRequestTypeListenEvent) == IOHIDAccessType.kIOHIDAccessTypeGranted;
        }

        private static bool checkPermission()
        {
            return checkInpuMonitoring() && checkAccessibility();
        }

        [STAThread]
        public static void Main(string[] args)
        {
            if (!checkPermission())
            {
                Console.WriteLine($"{Environment.OSVersion.Version}");

                _ = new Application(Eto.Platforms.Mac64);
                MessageBox.Show("OpenTabletDriver is likely missing OS permissions for Accessibility or Input Monitoring. " +
                    "To solve this, navigate to Settings -> System Preferences -> Security and Privacy -> Privacy, " +
                    "check both Accessibility and Input Monitoring. " +
                    "If they are already checked, uncheck and recheck them.", MessageBoxType.Warning);
                return;
            }

            App.Run(Eto.Platforms.Mac64, args);
        }
    }
}
