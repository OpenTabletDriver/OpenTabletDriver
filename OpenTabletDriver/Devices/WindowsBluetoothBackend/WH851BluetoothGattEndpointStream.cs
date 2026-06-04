using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace OpenTabletDriver.Devices.WindowsBluetoothBackend
{
    internal sealed class WH851BluetoothGattEndpointStream : IDeviceEndpointStream
    {
        private const int S_OK = 0;
        private const int HRESULT_ERROR_MORE_DATA = unchecked((int)0x800700EA);
        private const int ClientCharacteristicConfiguration = 2;
        private const int CharacteristicValueChangedEvent = 0;
        private const int MaxQueuedReports = 512;
        private const int OpenRetryCount = 5;
        private const int OpenRetryDelayMs = 250;
        private static readonly TimeSpan FirstReportWarningDelay = TimeSpan.FromSeconds(10);

        private readonly SafeFileHandle _serviceHandle;
        private readonly BlockingCollection<byte[]> _reports = new(new ConcurrentQueue<byte[]>(), MaxQueuedReports);
        private readonly List<IntPtr> _eventHandles = new();
        private readonly WindowsBluetoothGattNative.BluetoothGattEventCallback _callback;
        private readonly Timer _firstReportWarningTimer;
        private bool _disposed;
        private bool _firstReportLogged;

        private WH851BluetoothGattEndpointStream(SafeFileHandle serviceHandle)
        {
            _serviceHandle = serviceHandle;
            _callback = HandleGattEvent;
            _firstReportWarningTimer = new Timer(LogFirstReportMissingWarning, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

            try
            {
                InitializeNotifications();
                _firstReportWarningTimer.Change(FirstReportWarningDelay, Timeout.InfiniteTimeSpan);
            }
            catch
            {
                CleanupEventRegistrations();
                throw;
            }
        }

        public static IDeviceEndpointStream? Open(string devicePath)
        {
            Exception? lastException = null;

            for (var attempt = 1; attempt <= OpenRetryCount; attempt++)
            {
                var stream = TryOpen(devicePath, out lastException);
                if (stream is not null)
                    return stream;

                if (attempt != OpenRetryCount)
                    System.Threading.Thread.Sleep(OpenRetryDelayMs);
            }

            if (lastException is not null)
                Log.Exception(lastException, LogLevel.Debug);

            return null;
        }

        private static IDeviceEndpointStream? TryOpen(string devicePath, out Exception? exception)
        {
            var handle = WindowsBluetoothGattNative.CreateGattServiceHandle(devicePath);
            if (handle.IsInvalid)
            {
                handle.Dispose();
                exception = null;
                return null;
            }

            try
            {
                exception = null;
                return new WH851BluetoothGattEndpointStream(handle);
            }
            catch (Exception ex)
            {
                exception = ex;
                handle.Dispose();
                return null;
            }
        }

        public static bool CanOpenConnected(string devicePath)
        {
            using var handle = WindowsBluetoothGattNative.CreateGattServiceHandle(devicePath);
            if (handle.IsInvalid)
                return false;

            try
            {
                var inputCharacteristics = GetNotifiableCharacteristics(handle);
                return inputCharacteristics.Any(characteristic =>
                    EnableClientCharacteristicConfiguration(handle, characteristic, logFailures: false)
                );
            }
            catch (Exception ex)
            {
                Log.Debug("Bluetooth", $"Could not probe WH851 Bluetooth GATT notifications: {ex.Message}");
                return false;
            }
        }

        public byte[] Read()
        {
            try
            {
                return _reports.Take();
            }
            catch (InvalidOperationException ex)
            {
                throw new ObjectDisposedException(nameof(WH851BluetoothGattEndpointStream), ex);
            }
        }

        public void Write(byte[] buffer)
        {
        }

        public void GetFeature(byte[] buffer)
        {
        }

        public void SetFeature(byte[] buffer)
        {
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _reports.CompleteAdding();

            CleanupEventRegistrations();

            _firstReportWarningTimer.Dispose();
            _serviceHandle.Dispose();
        }

        private void InitializeNotifications()
        {
            var inputCharacteristics = GetNotifiableCharacteristics(_serviceHandle);

            if (inputCharacteristics.Length == 0)
                throw new IOException("No notifiable WH851 Bluetooth GATT characteristics were found.");

            var enabledNotificationCount = 0;
            foreach (var characteristic in inputCharacteristics)
            {
                RegisterValueChanged(characteristic);
                Log.Debug(
                    "Bluetooth",
                    $"Registered WH851 GATT characteristic {characteristic.CharacteristicUuid} notifications (notifiable={characteristic.IsNotifiable}, indicatable={characteristic.IsIndicatable})"
                );

                if (EnableClientCharacteristicConfiguration(_serviceHandle, characteristic, logFailures: true))
                {
                    enabledNotificationCount++;
                }
                else
                {
                    Log.Write(
                        "Bluetooth",
                        $"Could not explicitly enable WH851 Bluetooth GATT characteristic {characteristic.CharacteristicUuid} notifications; registering for value changes anyway.",
                        LogLevel.Warning
                    );
                }
            }

            if (enabledNotificationCount == 0)
                throw new IOException("No WH851 Bluetooth GATT notification subscriptions could be enabled. The device is likely not connected.");
        }

        private static WindowsBluetoothGattNative.BTH_LE_GATT_CHARACTERISTIC[] GetNotifiableCharacteristics(SafeFileHandle serviceHandle)
        {
            return GetCharacteristics(serviceHandle)
                .Where(c => c.IsNotifiable != 0 || c.IsIndicatable != 0)
                .ToArray();
        }

        private static WindowsBluetoothGattNative.BTH_LE_GATT_CHARACTERISTIC[] GetCharacteristics(SafeFileHandle serviceHandle)
        {
            var hr = WindowsBluetoothGattNative.BluetoothGATTGetCharacteristics(serviceHandle, IntPtr.Zero, 0, IntPtr.Zero, out var actual, 0);
            if (hr != HRESULT_ERROR_MORE_DATA && hr != S_OK)
                throw new IOException($"BluetoothGATTGetCharacteristics failed: 0x{hr:X8}");

            if (actual == 0)
                return Array.Empty<WindowsBluetoothGattNative.BTH_LE_GATT_CHARACTERISTIC>();

            var size = Marshal.SizeOf<WindowsBluetoothGattNative.BTH_LE_GATT_CHARACTERISTIC>();
            var buffer = Marshal.AllocHGlobal(size * actual);
            try
            {
                hr = WindowsBluetoothGattNative.BluetoothGATTGetCharacteristics(serviceHandle, IntPtr.Zero, actual, buffer, out actual, 0);
                if (hr != S_OK)
                    throw new IOException($"BluetoothGATTGetCharacteristics failed: 0x{hr:X8}");

                var characteristics = new WindowsBluetoothGattNative.BTH_LE_GATT_CHARACTERISTIC[actual];
                for (var i = 0; i < actual; i++)
                    characteristics[i] = Marshal.PtrToStructure<WindowsBluetoothGattNative.BTH_LE_GATT_CHARACTERISTIC>(buffer + i * size);

                return characteristics;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        private static WindowsBluetoothGattNative.BTH_LE_GATT_DESCRIPTOR[] GetDescriptors(SafeFileHandle serviceHandle, ref WindowsBluetoothGattNative.BTH_LE_GATT_CHARACTERISTIC characteristic)
        {
            var hr = WindowsBluetoothGattNative.BluetoothGATTGetDescriptors(serviceHandle, ref characteristic, 0, IntPtr.Zero, out var actual, 0);
            if (hr != HRESULT_ERROR_MORE_DATA && hr != S_OK)
                return Array.Empty<WindowsBluetoothGattNative.BTH_LE_GATT_DESCRIPTOR>();

            if (actual == 0)
                return Array.Empty<WindowsBluetoothGattNative.BTH_LE_GATT_DESCRIPTOR>();

            var size = Marshal.SizeOf<WindowsBluetoothGattNative.BTH_LE_GATT_DESCRIPTOR>();
            var buffer = Marshal.AllocHGlobal(size * actual);
            try
            {
                hr = WindowsBluetoothGattNative.BluetoothGATTGetDescriptors(serviceHandle, ref characteristic, actual, buffer, out actual, 0);
                if (hr != S_OK)
                    return Array.Empty<WindowsBluetoothGattNative.BTH_LE_GATT_DESCRIPTOR>();

                var descriptors = new WindowsBluetoothGattNative.BTH_LE_GATT_DESCRIPTOR[actual];
                for (var i = 0; i < actual; i++)
                    descriptors[i] = Marshal.PtrToStructure<WindowsBluetoothGattNative.BTH_LE_GATT_DESCRIPTOR>(buffer + i * size);

                return descriptors;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        private static bool EnableClientCharacteristicConfiguration(
            SafeFileHandle serviceHandle,
            WindowsBluetoothGattNative.BTH_LE_GATT_CHARACTERISTIC characteristic,
            bool logFailures)
        {
            var descriptors = GetDescriptors(serviceHandle, ref characteristic);
            var clientConfigurationFound = false;

            foreach (var descriptor in descriptors.Where(d => d.DescriptorType == ClientCharacteristicConfiguration))
            {
                clientConfigurationFound = true;
                var descriptorValue = WindowsBluetoothGattNative.BTH_LE_GATT_DESCRIPTOR_VALUE.CreateClientCharacteristicConfiguration(
                    descriptor.DescriptorUuid,
                    characteristic.IsNotifiable != 0,
                    characteristic.IsIndicatable != 0
                );
                var mutableDescriptor = descriptor;
                var hr = WindowsBluetoothGattNative.BluetoothGATTSetDescriptorValue(serviceHandle, ref mutableDescriptor, ref descriptorValue, 0);
                if (hr != S_OK)
                {
                    if (logFailures)
                        Log.Debug("Bluetooth", $"BluetoothGATTSetDescriptorValue failed: 0x{hr:X8}");

                    return false;
                }
            }

            return clientConfigurationFound;
        }

        private void RegisterValueChanged(WindowsBluetoothGattNative.BTH_LE_GATT_CHARACTERISTIC characteristic)
        {
            var registration = new WindowsBluetoothGattNative.BLUETOOTH_GATT_VALUE_CHANGED_EVENT_REGISTRATION
            {
                NumCharacteristics = 1,
                Characteristic = characteristic
            };

            var hr = WindowsBluetoothGattNative.BluetoothGATTRegisterEvent(
                _serviceHandle,
                CharacteristicValueChangedEvent,
                ref registration,
                _callback,
                IntPtr.Zero,
                out var eventHandle,
                0
            );

            if (hr != S_OK)
                throw new IOException($"BluetoothGATTRegisterEvent failed: 0x{hr:X8}");

            _eventHandles.Add(eventHandle);
        }

        private void CleanupEventRegistrations()
        {
            foreach (var eventHandle in _eventHandles)
                WindowsBluetoothGattNative.BluetoothGATTUnregisterEvent(eventHandle, 0);

            _eventHandles.Clear();
        }

        private void HandleGattEvent(int eventType, IntPtr eventOutParameter, IntPtr context)
        {
            if (_disposed || eventType != CharacteristicValueChangedEvent || eventOutParameter == IntPtr.Zero)
                return;

            try
            {
                var valueChangedEvent = Marshal.PtrToStructure<WindowsBluetoothGattNative.BLUETOOTH_GATT_VALUE_CHANGED_EVENT>(eventOutParameter);
                var valuePointer = valueChangedEvent.CharacteristicValue;
                if (valuePointer == IntPtr.Zero)
                    return;

                var dataSize = Marshal.ReadInt32(valuePointer, 0);
                if (dataSize <= 0 || dataSize > 256)
                    return;

                var report = new byte[dataSize];
                Marshal.Copy(valuePointer + WindowsBluetoothGattNative.BTH_LE_GATT_CHARACTERISTIC_VALUE_DATA_OFFSET, report, 0, dataSize);

                if (!_firstReportLogged)
                {
                    _firstReportLogged = true;
                    _firstReportWarningTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                    Log.Debug("Bluetooth", $"Received first WH851 Bluetooth GATT report (length={report.Length}).");
                }

                if (!_reports.IsAddingCompleted && !_reports.TryAdd(report))
                    Log.Debug("Bluetooth", "Dropped WH851 Bluetooth report because the input queue is full.");
            }
            catch (Exception ex)
            {
                Log.Exception(ex, LogLevel.Debug);
            }
        }

        private void LogFirstReportMissingWarning(object? state)
        {
            if (_disposed || _firstReportLogged)
                return;

            Log.Write(
                "Bluetooth",
                "WH851 Bluetooth GATT endpoint initialized, but no input reports were received yet. Wake the tablet with the pen and avoid using USB and Bluetooth at the same time while diagnosing.",
                LogLevel.Warning
            );
        }
    }
}



