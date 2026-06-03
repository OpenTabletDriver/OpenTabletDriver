using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace OpenTabletDriver.Devices.WindowsBluetoothBackend
{
    internal sealed class WindowsBluetoothGattDeviceInterfaceInfo
    {
        public WindowsBluetoothGattDeviceInterfaceInfo(string devicePath, IReadOnlyList<string> hardwareIds)
        {
            DevicePath = devicePath;
            HardwareIds = hardwareIds;
            BluetoothAddress = GetBluetoothAddress(hardwareIds) ?? GetBluetoothAddress(devicePath);
        }

        public string DevicePath { get; }
        public IReadOnlyList<string> HardwareIds { get; }
        public string? BluetoothAddress { get; }

        public bool HasHardwareId(int vendorId, int productId)
        {
            var expected = $"vid&02{vendorId:x4}_pid&{productId:x4}";
            return HardwareIds.Any(id => id.Contains(expected, StringComparison.OrdinalIgnoreCase));
        }

        private static string? GetBluetoothAddress(IEnumerable<string> values)
        {
            return values.Select(GetBluetoothAddress).FirstOrDefault(value => value is not null);
        }

        private static string? GetBluetoothAddress(string value)
        {
            var match = Regex.Match(value, @"_([0-9a-f]{12})(?:\\|#|$)", RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value : null;
        }
    }

    internal static class WindowsBluetoothGattDeviceInterfaceEnumerator
    {
        private const int DIGCF_PRESENT = 2;
        private const int DIGCF_ALLCLASSES = 4;
        private const int DIGCF_DEVICEINTERFACE = 16;
        private const int SPDRP_HARDWAREID = 1;
        private const int ERROR_NO_MORE_ITEMS = 259;

        private static readonly Guid BluetoothLeHidServiceUuid = Guid.Parse("00001812-0000-1000-8000-00805F9B34FB");

        public static IEnumerable<WindowsBluetoothGattDeviceInterfaceInfo> EnumerateDeviceInterfacePaths(Guid interfaceGuid)
        {
            var deviceInfoSet = SetupDiGetClassDevs(in interfaceGuid, IntPtr.Zero, IntPtr.Zero, DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);
            if (deviceInfoSet == IntPtr.Zero || deviceInfoSet == new IntPtr(-1))
                yield break;

            try
            {
                for (uint index = 0; ; index++)
                {
                    var interfaceData = SP_DEVICE_INTERFACE_DATA.Create();
                    if (!SetupDiEnumDeviceInterfaces(deviceInfoSet, IntPtr.Zero, in interfaceGuid, index, ref interfaceData))
                    {
                        if (Marshal.GetLastWin32Error() == ERROR_NO_MORE_ITEMS)
                            yield break;

                        continue;
                    }

                    var info = GetDeviceInterfaceInfo(deviceInfoSet, ref interfaceData);
                    if (info is not null)
                        yield return info;
                }
            }
            finally
            {
                SetupDiDestroyDeviceInfoList(deviceInfoSet);
            }
        }

        public static bool HasPresentBluetoothLeHidChild(string? bluetoothAddress, int vendorId, int productId)
        {
            if (string.IsNullOrWhiteSpace(bluetoothAddress))
                return false;

            var deviceInfoSet = SetupDiGetClassDevs(IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, DIGCF_PRESENT | DIGCF_ALLCLASSES);
            if (deviceInfoSet == IntPtr.Zero || deviceInfoSet == new IntPtr(-1))
                return false;

            try
            {
                for (uint index = 0; ; index++)
                {
                    var deviceInfoData = SP_DEVINFO_DATA.Create();
                    if (!SetupDiEnumDeviceInfo(deviceInfoSet, index, ref deviceInfoData))
                    {
                        if (Marshal.GetLastWin32Error() == ERROR_NO_MORE_ITEMS)
                            return false;

                        continue;
                    }

                    var instanceId = GetDeviceInstanceId(deviceInfoSet, ref deviceInfoData);
                    if (string.IsNullOrWhiteSpace(instanceId) || !instanceId.Contains(bluetoothAddress, StringComparison.OrdinalIgnoreCase))
                        continue;

                    var hardwareIds = GetHardwareIds(deviceInfoSet, ref deviceInfoData);
                    if (IsBluetoothLeHidChild(hardwareIds, vendorId, productId))
                        return true;
                }
            }
            finally
            {
                SetupDiDestroyDeviceInfoList(deviceInfoSet);
            }
        }

        private static WindowsBluetoothGattDeviceInterfaceInfo? GetDeviceInterfaceInfo(IntPtr deviceInfoSet, ref SP_DEVICE_INTERFACE_DATA interfaceData)
        {
            _ = SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref interfaceData, IntPtr.Zero, 0, out var requiredSize, IntPtr.Zero);
            if (requiredSize == 0)
                return null;

            var detailData = Marshal.AllocHGlobal((int)requiredSize);
            try
            {
                Marshal.WriteInt32(detailData, IntPtr.Size == 8 ? 8 : 4 + Marshal.SystemDefaultCharSize);
                var deviceInfoData = SP_DEVINFO_DATA.Create();
                if (!SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref interfaceData, detailData, requiredSize, out _, ref deviceInfoData))
                    return null;

                var path = Marshal.PtrToStringUni(detailData + 4);
                if (string.IsNullOrWhiteSpace(path))
                    return null;

                return new WindowsBluetoothGattDeviceInterfaceInfo(path, GetHardwareIds(deviceInfoSet, ref deviceInfoData));
            }
            finally
            {
                Marshal.FreeHGlobal(detailData);
            }
        }

        private static IReadOnlyList<string> GetHardwareIds(IntPtr deviceInfoSet, ref SP_DEVINFO_DATA deviceInfoData)
        {
            _ = SetupDiGetDeviceRegistryProperty(deviceInfoSet, ref deviceInfoData, SPDRP_HARDWAREID, out _, IntPtr.Zero, 0, out var requiredSize);
            if (requiredSize == 0)
                return Array.Empty<string>();

            var buffer = Marshal.AllocHGlobal((int)requiredSize);
            try
            {
                if (!SetupDiGetDeviceRegistryProperty(deviceInfoSet, ref deviceInfoData, SPDRP_HARDWAREID, out _, buffer, requiredSize, out _))
                    return Array.Empty<string>();

                var hardwareIdMultiString = Marshal.PtrToStringUni(buffer, (int)requiredSize / sizeof(char));
                return hardwareIdMultiString?
                    .Split('\0', StringSplitOptions.RemoveEmptyEntries)
                    .ToArray() ?? Array.Empty<string>();
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        private static string? GetDeviceInstanceId(IntPtr deviceInfoSet, ref SP_DEVINFO_DATA deviceInfoData)
        {
            _ = SetupDiGetDeviceInstanceId(deviceInfoSet, ref deviceInfoData, IntPtr.Zero, 0, out var requiredSize);
            if (requiredSize == 0)
                return null;

            var buffer = Marshal.AllocHGlobal((int)requiredSize * sizeof(char));
            try
            {
                return SetupDiGetDeviceInstanceId(deviceInfoSet, ref deviceInfoData, buffer, requiredSize, out _)
                    ? Marshal.PtrToStringUni(buffer)
                    : null;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        private static bool IsBluetoothLeHidChild(IReadOnlyList<string> hardwareIds, int vendorId, int productId)
        {
            var expectedDevice = $"hid\\{{{BluetoothLeHidServiceUuid}}}_dev_vid&02{vendorId:x4}_pid&{productId:x4}";
            return hardwareIds.Any(id => id.StartsWith(expectedDevice, StringComparison.OrdinalIgnoreCase));
        }

        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern IntPtr SetupDiGetClassDevs(in Guid classGuid, IntPtr enumerator, IntPtr hwndParent, int flags);

        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern IntPtr SetupDiGetClassDevs(IntPtr classGuid, IntPtr enumerator, IntPtr hwndParent, int flags);

        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern bool SetupDiEnumDeviceInterfaces(IntPtr deviceInfoSet, IntPtr deviceInfoData, in Guid interfaceClassGuid, uint memberIndex, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData);

        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern bool SetupDiEnumDeviceInfo(IntPtr deviceInfoSet, uint memberIndex, ref SP_DEVINFO_DATA deviceInfoData);

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr deviceInfoSet, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData, IntPtr deviceInterfaceDetailData, uint deviceInterfaceDetailDataSize, out uint requiredSize, IntPtr deviceInfoData);

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr deviceInfoSet, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData, IntPtr deviceInterfaceDetailData, uint deviceInterfaceDetailDataSize, out uint requiredSize, ref SP_DEVINFO_DATA deviceInfoData);

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool SetupDiGetDeviceRegistryProperty(IntPtr deviceInfoSet, ref SP_DEVINFO_DATA deviceInfoData, uint property, out uint propertyRegDataType, IntPtr propertyBuffer, uint propertyBufferSize, out uint requiredSize);

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool SetupDiGetDeviceInstanceId(IntPtr deviceInfoSet, ref SP_DEVINFO_DATA deviceInfoData, IntPtr deviceInstanceId, uint deviceInstanceIdSize, out uint requiredSize);

        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern bool SetupDiDestroyDeviceInfoList(IntPtr deviceInfoSet);

        [StructLayout(LayoutKind.Sequential)]
        private struct SP_DEVICE_INTERFACE_DATA
        {
            public int Size;
            public Guid InterfaceClassGuid;
            public int Flags;
            private readonly UIntPtr _reserved;

            public static SP_DEVICE_INTERFACE_DATA Create()
            {
                return new SP_DEVICE_INTERFACE_DATA { Size = Marshal.SizeOf<SP_DEVICE_INTERFACE_DATA>() };
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SP_DEVINFO_DATA
        {
            public int Size;
            public Guid ClassGuid;
            public uint DevInst;
            private readonly UIntPtr _reserved;

            public static SP_DEVINFO_DATA Create()
            {
                return new SP_DEVINFO_DATA { Size = Marshal.SizeOf<SP_DEVINFO_DATA>() };
            }
        }
    }
}
