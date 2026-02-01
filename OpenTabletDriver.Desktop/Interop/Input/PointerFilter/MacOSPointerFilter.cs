using System;
using System.Collections.Generic;
using System.Threading;
using OpenTabletDriver.Native.OSX;
using OpenTabletDriver.Native.OSX.Input;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Interop.Input.Filter
{
    using static CoreFoundation;
    using static OSX;

    public class MacOSPointerFilter : ISystemPointerFilter
    {
        public bool Enabled { get => _filterEnabled; }

        private static readonly Dictionary<long, (int vendorId, int productId)> _vendorMap = new();
        private static HashSet<(int vendorId, int productId)> _deviceInfoHash = new();
        private Dictionary<string, bool> _connections = new();

        private Thread _pointerThread;

        private static DateTime _lastTabletEvent;
        private IntPtr _tap;
        private IntPtr _runLoop;
        private IntPtr _runLoopSource;

        private bool _filterEnabled = false;

        private const CGEventTypeMask _allMouseEvents =
            CGEventTypeMask.MouseMoved |
            CGEventTypeMask.LeftMouseDown |
            CGEventTypeMask.RightMouseDown |
            CGEventTypeMask.LeftMouseDragged |
            CGEventTypeMask.RightMouseDragged;

        public MacOSPointerFilter()
        {
        }

        public void ConnectionStatusChanged(string deviceName, List<DeviceIdentifier> identifiers, bool connected)
        {
            var isDeviceAlreadyAdded = _connections.ContainsKey(deviceName);
            if (connected && !isDeviceAlreadyAdded)
            {
                _connections.Add(deviceName, connected);

                AddDeviceInfo(identifiers);
                EnableFilter();
            }
            else if (!connected && isDeviceAlreadyAdded)
            {
                _connections.Remove(deviceName);
                RemoveDeviceInfo(identifiers);
                if (_connections.Count == 0)
                {
                    DisableFilter();
                }
            }
        }

        private void AddDeviceInfo(List<DeviceIdentifier> identifiers)
        {
            foreach (DeviceIdentifier identifier in identifiers)
            {
                var infoAlreadyExists = _deviceInfoHash.Contains((identifier.VendorID, identifier.ProductID));
                if (!infoAlreadyExists)
                {
                    _deviceInfoHash.Add((identifier.VendorID, identifier.ProductID));
                }
            }
        }

        private void RemoveDeviceInfo(List<DeviceIdentifier> identifiers)
        {
            foreach (DeviceIdentifier identifier in identifiers)
            {
                var infoAlreadyExists = _deviceInfoHash.Contains((identifier.VendorID, identifier.ProductID));
                if (infoAlreadyExists)
                {
                    _deviceInfoHash.Remove((identifier.VendorID, identifier.ProductID));
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
            _pointerThread.IsBackground = true;
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

        private static void StoreTabletIdentifiers(CGEventType type, IntPtr @event)
        {
            var vendorId = (int)CGEventGetIntegerValueField(@event, CGEventField.tabletProximityEventVendorID);
            var productId = (int)CGEventGetIntegerValueField(@event, CGEventField.tabletProximityEventTabletID);
            var deviceId = CGEventGetIntegerValueField(@event, CGEventField.tabletProximityEventDeviceID);

            if (vendorId != 0 && deviceId != 0 && productId != 0)
            {
                _vendorMap[deviceId] = (vendorId, productId);
            }
        }

        private static IntPtr EventCallback(IntPtr proxy, CGEventType type, IntPtr @event, IntPtr userInfo)
        {
            var pid = CGEventGetIntegerValueField(@event, CGEventField.eventSourceUnixProcessID);
            if (type == CGEventType.kCGEventTabletProximity)
            {
                // In order to only filter unmanaged tablet events, we need to fetch the vendor, device and product id
                // These can only be fetched during a proximity event.
                StoreTabletIdentifiers(type, @event);
            }

            if (pid == Environment.ProcessId)
            {
                _lastTabletEvent = DateTime.UtcNow;
                return @event;
            }
            else if (pid == 0 && TabletRecentlyActive())
            {
                var deviceId = CGEventGetIntegerValueField(@event, CGEventField.tabletEventDeviceID);

                if (IsMouseEvent(type))
                {
                    // Because there is no way to see where mouse events originate from
                    // we filter all of them in the timespan where the tablet is also active.
                    return IntPtr.Zero;
                }
                else if (type == CGEventType.kCGEventTabletPointer && _vendorMap.TryGetValue(deviceId, out var data) && _deviceInfoHash.Contains(data))
                {
                    return IntPtr.Zero;
                }
            }

            return @event;
        }

        private static bool TabletRecentlyActive()
        {
            return (DateTime.UtcNow - _lastTabletEvent) < TimeSpan.FromMilliseconds(33);
        }

        private static bool IsMouseEvent(CGEventType type)
        {
            CGEventTypeMask typeMask = (CGEventTypeMask)(1UL << (int)type);
            return (_allMouseEvents & typeMask) != 0;
        }
    }
}
