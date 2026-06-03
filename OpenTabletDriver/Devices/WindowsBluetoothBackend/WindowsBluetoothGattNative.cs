using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace OpenTabletDriver.Devices.WindowsBluetoothBackend
{
    internal static class WindowsBluetoothGattNative
    {
        // BTH_LE_GATT_CHARACTERISTIC_VALUE is declared in bthledef.h as:
        // ULONG DataSize; UCHAR Data[1];
        // The notification callback gives us a pointer to that structure, so
        // report bytes begin immediately after the 4-byte DataSize field.
        public const int BTH_LE_GATT_CHARACTERISTIC_VALUE_DATA_OFFSET = 4;

        private const uint GENERIC_READ = 0x80000000;
        private const uint GENERIC_WRITE = 0x40000000;
        private const uint FILE_SHARE_READ = 1;
        private const uint FILE_SHARE_WRITE = 2;
        private const uint OPEN_EXISTING = 3;

        public delegate void BluetoothGattEventCallback(int eventType, IntPtr eventOutParameter, IntPtr context);

        public static SafeFileHandle CreateGattServiceHandle(string path)
        {
            return CreateFile(path, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern SafeFileHandle CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("BluetoothAPIs.dll", SetLastError = true)]
        public static extern int BluetoothGATTGetCharacteristics(SafeFileHandle serviceHandle, IntPtr service, ushort characteristicsBufferCount, IntPtr characteristicsBuffer, out ushort characteristicsBufferActual, uint flags);

        [DllImport("BluetoothAPIs.dll", SetLastError = true)]
        public static extern int BluetoothGATTGetDescriptors(SafeFileHandle serviceHandle, ref BTH_LE_GATT_CHARACTERISTIC characteristic, ushort descriptorsBufferCount, IntPtr descriptorsBuffer, out ushort descriptorsBufferActual, uint flags);

        [DllImport("BluetoothAPIs.dll", SetLastError = true)]
        public static extern int BluetoothGATTSetDescriptorValue(SafeFileHandle serviceHandle, ref BTH_LE_GATT_DESCRIPTOR descriptor, ref BTH_LE_GATT_DESCRIPTOR_VALUE descriptorValue, uint flags);

        [DllImport("BluetoothAPIs.dll", SetLastError = true)]
        public static extern int BluetoothGATTRegisterEvent(SafeFileHandle serviceHandle, int eventType, ref BLUETOOTH_GATT_VALUE_CHANGED_EVENT_REGISTRATION eventParameterIn, BluetoothGattEventCallback callback, IntPtr callbackContext, out IntPtr eventHandle, uint flags);

        [DllImport("BluetoothAPIs.dll", SetLastError = true)]
        public static extern int BluetoothGATTUnregisterEvent(IntPtr eventHandle, uint flags);

        [StructLayout(LayoutKind.Sequential)]
        public struct BTH_LE_UUID
        {
            public byte IsShortUuid;
            private readonly byte _padding1;
            private readonly byte _padding2;
            private readonly byte _padding3;
            public BTH_LE_UUID_VALUE Value;

            public override string ToString()
            {
                return IsShortUuid != 0 ? $"{Value.ShortUuid:X4}" : Value.LongUuid.ToString();
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct BTH_LE_UUID_VALUE
        {
            [FieldOffset(0)]
            public ushort ShortUuid;

            [FieldOffset(0)]
            public Guid LongUuid;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BTH_LE_GATT_CHARACTERISTIC
        {
            public ushort ServiceHandle;
            public BTH_LE_UUID CharacteristicUuid;
            public ushort AttributeHandle;
            public ushort CharacteristicValueHandle;
            public byte IsBroadcastable;
            public byte IsReadable;
            public byte IsWritable;
            public byte IsWritableWithoutResponse;
            public byte IsSignedWritable;
            public byte IsNotifiable;
            public byte IsIndicatable;
            public byte HasExtendedProperties;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BTH_LE_GATT_DESCRIPTOR
        {
            public ushort ServiceHandle;
            public ushort CharacteristicHandle;
            public int DescriptorType;
            public BTH_LE_UUID DescriptorUuid;
            public ushort AttributeHandle;
        }

        private const int BTH_LE_GATT_DESCRIPTOR_VALUE_SIZE = 80;
        private const int DESCRIPTOR_VALUE_DESCRIPTOR_TYPE_OFFSET = 0;
        private const int DESCRIPTOR_VALUE_DESCRIPTOR_UUID_OFFSET = 4;
        private const int DESCRIPTOR_VALUE_CCC_NOTIFICATION_OFFSET = 24;
        private const int DESCRIPTOR_VALUE_CCC_INDICATION_OFFSET = 25;
        private const int DESCRIPTOR_VALUE_DATA_SIZE_OFFSET = 72;

        // Mirrors BTH_LE_GATT_DESCRIPTOR_VALUE from bthledef.h.
        //
        // The Windows SDK structure contains a BTH_LE_UUID followed by a large
        // union. For WH851 we only populate the ClientCharacteristicConfiguration
        // union arm used by BluetoothGATTSetDescriptorValue to enable GATT
        // notifications. The explicit offsets below are covered by
        // WH851TransportTests so future runtime/SDK layout changes fail fast.
        [StructLayout(LayoutKind.Explicit, Size = BTH_LE_GATT_DESCRIPTOR_VALUE_SIZE)]
        public struct BTH_LE_GATT_DESCRIPTOR_VALUE
        {
            [FieldOffset(DESCRIPTOR_VALUE_DESCRIPTOR_TYPE_OFFSET)]
            public int DescriptorType;

            [FieldOffset(DESCRIPTOR_VALUE_DESCRIPTOR_UUID_OFFSET)]
            public BTH_LE_UUID DescriptorUuid;

            [FieldOffset(DESCRIPTOR_VALUE_CCC_NOTIFICATION_OFFSET)]
            public byte IsSubscribeToNotification;

            [FieldOffset(DESCRIPTOR_VALUE_CCC_INDICATION_OFFSET)]
            public byte IsSubscribeToIndication;

            [FieldOffset(DESCRIPTOR_VALUE_DATA_SIZE_OFFSET)]
            public uint DataSize;

            public static BTH_LE_GATT_DESCRIPTOR_VALUE CreateClientCharacteristicConfiguration(BTH_LE_UUID descriptorUuid, bool notify, bool indicate)
            {
                return new BTH_LE_GATT_DESCRIPTOR_VALUE
                {
                    DescriptorType = 2,
                    DescriptorUuid = descriptorUuid,
                    IsSubscribeToNotification = notify ? (byte)1 : (byte)0,
                    IsSubscribeToIndication = indicate ? (byte)1 : (byte)0,
                    DataSize = 0
                };
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BLUETOOTH_GATT_VALUE_CHANGED_EVENT_REGISTRATION
        {
            public ushort NumCharacteristics;
            private readonly ushort _padding;
            public BTH_LE_GATT_CHARACTERISTIC Characteristic;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BLUETOOTH_GATT_VALUE_CHANGED_EVENT
        {
            public ushort ChangedAttributeHandle;
            private readonly ushort _padding1;
            private readonly uint _padding2;
            public UIntPtr CharacteristicValueDataSize;
            public IntPtr CharacteristicValue;
        }
    }
}
