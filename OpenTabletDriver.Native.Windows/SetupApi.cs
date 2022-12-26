using System;
using System.Runtime.InteropServices;
using OpenTabletDriver.Native.Windows.SetupApiStructs;
using OpenTabletDriver.Native.Windows.USB;

namespace OpenTabletDriver.Native.Windows
{
    public static partial class SetupAPI
    {
        public const int ERROR_NO_MORE_ITEMS = 259;

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr SetupDiGetClassDevs(in Guid classGuid, [MarshalAs(UnmanagedType.LPWStr)] string enumerator, IntPtr hwndParent, DIGCF flags);

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr SetupDiGetClassDevs(in Guid classGuid, IntPtr enumerator, IntPtr hwndParent, DIGCF flags);

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool SetupDiEnumDeviceInterfaces(IntPtr hDevInfo, IntPtr devInfo, in Guid interfaceClassGuid, uint memberIndex, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData);

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr hDevInfo, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData, ref SP_DEVICE_INTERFACE_DETAIL_DATA deviceInterfaceDetailData, uint deviceInterfaceDetailDataSize, ref uint requiredSize, IntPtr deviceInfoData);

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr hDevInfo, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData, IntPtr deviceInterfaceDetailData, uint deviceInterfaceDetailDataSize, ref uint requiredSize, IntPtr deviceInfoData);

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool SetupDiDestroyDeviceInfoList(IntPtr deviceInfoSet);
    }
}
