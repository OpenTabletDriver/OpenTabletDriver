using System;
using System.Collections.Generic;
using System.Threading;
using OpenTabletDriver.Native.OSX;
using OpenTabletDriver.Native.OSX.Input;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Interop.Input.Filter
{
    using static CoreFoundation;
    using static OSX;

    public class MacOSPointerFilter : ISystemPointerFilter
    {
        public bool Enabled { get => _filterEnabled; }

        private Dictionary<string, bool> _connections = new Dictionary<string, bool>();

        private Thread _pointerThread;

        private static DateTime _lastTabletEvent;
        private IntPtr _tap;
        private IntPtr _runLoop;
        private IntPtr _runLoopSource;

        private bool _filterEnabled = false;

        public MacOSPointerFilter()
        {
        }

        public void ConnectionStatusChanged(string deviceId, bool connected)
        {
            var deviceAdded = _connections.ContainsKey(deviceId);
            if (connected && !deviceAdded)
            {
                _connections.Add(deviceId, connected);

                EnableFilter();
            }
            else if (deviceAdded)
            {
                _connections.Remove(deviceId);
                if (_connections.Count == 0)
                {
                    DisableFilter();
                }
            }
        }

        private void EnableFilter()
        {
            if (_filterEnabled) { return; }

            _pointerThread = new Thread(() =>
            {

                _tap = CGEventTapCreate(
                    CGEventTapLocation.kCGHIDEventTap,
                    CGEventTapPlacement.kCGHeadInsertEventTap,
                    CGEventTapOptions.kCGEventTapOptionDefault,
                    CGEventTypeMask.MouseMoved |
                    CGEventTypeMask.LeftMouseDown |
                    CGEventTypeMask.LeftMouseDragged |
                    CGEventTypeMask.RightMouseDown |
                    CGEventTypeMask.RightMouseDragged |
                    CGEventTypeMask.TabletPointer |
                    CGEventTypeMask.TabletProximity,
                    EventCallback,
                    IntPtr.Zero
                );

                _runLoop = CFRunLoopGetCurrent();
                _runLoopSource = CFMachPortCreateRunLoopSource(IntPtr.Zero, _tap, 0);

                CFRunLoopAddSource(_runLoop, _runLoopSource, kCFRunLoopCommonModes);
                CGEventTapEnable(_tap, true);

                CFRunLoopRun();
            });

            _pointerThread.Start();

            _filterEnabled = true;

            Log.Debug("MacOSPointerFilter", "Filter Enabled");
        }

        private void DisableFilter()
        {
            if (!_filterEnabled) { return; }

            if (_tap != IntPtr.Zero)
            {
                CGEventTapEnable(_tap, false);
                CFRelease(_tap);
                _tap = IntPtr.Zero;
            }

            if (_runLoop != IntPtr.Zero)
            {
                CFRunLoopStop(_runLoop);
                _runLoop = IntPtr.Zero;
            }

            if (_pointerThread != null && _pointerThread.IsAlive)
            {
                _pointerThread.Join(TimeSpan.FromSeconds(2));
            }

            if (_runLoopSource != IntPtr.Zero)
            {
                CFRelease(_runLoopSource);
                _runLoopSource = IntPtr.Zero;
            }

            _filterEnabled = false;

            Log.Debug("MacOSPointerFilter", "Filter Disabled");
        }

        private static IntPtr EventCallback(IntPtr proxy, CGEventTypeMask type, IntPtr @event, IntPtr userInfo)
        {
            var pid = CGEventGetIntegerValueField(@event, CGEventField.eventSourceUnixProcessID);

            if (pid == Environment.ProcessId)
            {
                _lastTabletEvent = DateTime.UtcNow;
                return @event;
            }
            else if (pid == 0 && TabletRecentlyActive())
            {
                // Ignore events coming from system when the tablet is also active.
                return IntPtr.Zero;
            }

            return @event;
        }

        private static bool TabletRecentlyActive()
        {
            return (DateTime.UtcNow - _lastTabletEvent) < TimeSpan.FromMilliseconds(150);
        }
    }
}
