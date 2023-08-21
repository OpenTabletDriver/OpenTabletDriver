using System;
using System.Diagnostics;
using System.Threading;
using MonoMac.AppKit;
using MonoMac.Foundation;
using MonoMac.ObjCRuntime;
using OpenTabletDriver.Native.MacOS;
using OpenTabletDriver.Native.MacOS.ApplicationServices;
using OpenTabletDriver.Native.MacOS.IOkit;

namespace OpenTabletDriver.UX.MacOS
{
    static public class PermissionHelper
    {
        public static bool HasPermissions()
        {
            NSApplication.Init();

            //https://source.chromium.org/chromium/chromium/src/+/refs/heads/main:base/process/process_info_mac.cc;drc=b0e89488c257ea10c5b433b7ff037150bacbade6;l=13
            //Permissions are inherited from parent application such as Terminal, Visual Studio if the app is not launched from finder/dock/etc...
            var responsiblePid = LibQuarantine.responsibility_get_pid_responsible_for_pid(Environment.ProcessId);
            var selfResponsible = responsiblePid == Environment.ProcessId;
            var responsibleAppName = NSRunningApplication.GetRunningApplication(responsiblePid)?.LocalizedName ?? "OpenTabletDriver";

            //https://openradar.appspot.com/7381305
            //Have to call IOHIDRequestAccess before AXIsProcessTrusted
            var hasInputMonitoring = Environment.OSVersion.Version < new Version(10, 15) ||
                    IOKit.IOHIDCheckAccess(IOHIDRequestType.kIOHIDRequestTypeListenEvent) == IOHIDAccessType.kIOHIDAccessTypeGranted;

            if (!hasInputMonitoring)
            {
                handlePermission(
                    type: "Input Monitoring",
                    message: "To use OpenTabletDriver, you need to grant the \"Input Monitoring\" permission so that input from the tablet can be received By OpenTabletDriover.\n\n" +
                        $"To enable this permission, please click on \"Open Input Monitoring Preferences\" below. In the Input Preferences Monitoring panel that opens, tick the box next to \"{responsibleAppName}\".\n\n" +
                        $"If \"{responsibleAppName}\" is already selected, you can remove it from the list by clicking on the \"-\" button, and then add it back to the list.",
                    preferencesLink: "x-apple.systempreferences:com.apple.preference.security?Privacy_ListenEvent",
                    requestAccess: () =>
                    {
                        //When granting permission, system will ask the user to quit and restart the app.
                        //Catch the quit event here and kill and restart the app.
                        KillOnQuitHandler.Register();

                        IOKit.IOHIDRequestAccess(IOHIDRequestType.kIOHIDRequestTypeListenEvent);
                    });
            }

            var hasAccessibility = ApplicationServices.AXIsProcessTrusted();

            if (!hasAccessibility)
            {
                handlePermission(
                    type: "Accessibility",
                    message: $"To use OpenTabletDriver, you need to grant the \"Accessibility\" permission so that input from the tablet can be injected into the computer. \n\n" +
                        $"To enable this permission, please click on \"Open Accessibility Preferences\" below. In the Accessibility Preferences panel that opens, tick the box next to \"{responsibleAppName}\".\n\n" +
                        $"If \"{responsibleAppName}\" is already selected, you can remove it from the list by clicking on the \"-\" button, and then add it back to the list.",
                    preferencesLink: "x-apple.systempreferences:com.apple.preference.security?Privacy_Accessibility",
                    requestAccess: () => axRequestAccess());
            }

            if (!hasAccessibility || !hasInputMonitoring)
            {
                if (selfResponsible)
                {
                    var process = Process.Start("open", new[] { NSBundle.MainBundle.BundlePath, "-n" });
                    process.WaitForExit();
                }
                else
                {
                    var alert = new NSAlert()
                    {
                        MessageText = $"Please restart {responsibleAppName} and launch OpenTabletDriver again.",
                        AlertStyle = NSAlertStyle.Warning,
                    };
                    alert.RunModal();
                }

                return false;
            }
            return true;
        }

        private static void handlePermission(
            string type,
            string message,
            string preferencesLink,
            Action requestAccess
            )
        {
            var alert = new NSAlert()
            {
                MessageText = message,
                AlertStyle = NSAlertStyle.Warning,
            };

            alert.AddButton($"Next");
            alert.AddButton($"Open {type} Preference");
            alert.AddButton($"Cancel");

            var linkHandler = new UrlHandler(preferencesLink);
            alert.Buttons[1].Action = new Selector("onClick:");
            alert.Buttons[1].Target = linkHandler;

            var cancelHandler = new CancelHandler();
            alert.Buttons[2].Action = new Selector("onClick:");
            alert.Buttons[2].Target = cancelHandler;

            //Prevent our alert obscuring the system permission dialog.
            Thread thread = new Thread(() =>
            {
                Thread.Sleep(150);
                NSApplication.SharedApplication.InvokeOnMainThread(() =>
                {
                    alert.Window.Level = NSWindowLevel.Normal;
                    //Prevent the console app/visual studio obscuring our alert
                    alert.Window.OrderFrontRegardless();
                    requestAccess();
                });
            });

            thread.Start();
            var result = alert.RunModal();
            thread.Join();
        }

        private static void axRequestAccess()
        {
            var options = CoreFoundation.CFDictionaryCreateMutable(IntPtr.Zero, 0, IntPtr.Zero, IntPtr.Zero);
            CoreFoundation.CFDictionaryAddValue(options, ApplicationServices.kAXTrustedCheckOptionPrompt, CoreFoundation.kCFBooleanTrue);
            ApplicationServices.AXIsProcessTrustedWithOptions(options);
        }

        private class KillOnQuitHandler : NSObject
        {
            static KillOnQuitHandler handler;

            [Export("handleQuitEvent:withReplyEvent:")]
            private void handleQuitEvent(NSAppleEventDescriptor evt, NSAppleEventDescriptor replyEvt)
            {
                Process.GetCurrentProcess().Kill();
            }

            public static void Register()
            {
                handler ??= new KillOnQuitHandler();
                NSAppleEventManager.SharedAppleEventManager.SetEventHandler(handler,
                    new MonoMac.ObjCRuntime.Selector("handleQuitEvent:withReplyEvent:"), AEEventClass.AppleEvent,
                    AEEventID.QuitApplication);
            }
        }

        private class UrlHandler : NSObject
        {
            public UrlHandler(string url)
            {
                _url = url;
            }

            private string _url;

            [Export("onClick:")]
            private void onClick(NSObject target)
            {
                NSWorkspace.SharedWorkspace.OpenUrl(new NSUrl(_url));
            }
        }

        private class CancelHandler : NSObject
        {
            public CancelHandler()
            {
            }

            [Export("onClick:")]
            private void onClick(NSObject target)
            {
                Process.GetCurrentProcess().Kill();
            }
        }
    }
}
