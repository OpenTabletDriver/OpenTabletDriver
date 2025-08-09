using System;
using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.Windows.CM
{
    [StructLayout(LayoutKind.Explicit)]
    public unsafe ref struct CM_NOTIFY_EVENT_DATA
    {
        [FieldOffset(0)]
        public CM_NOTIFY_FILTER_TYPE FilterType;

        [FieldOffset(4)]
        public uint Reserved;

        [FieldOffset(8)]
        public Guid ClassGuid;

        [FieldOffset(24)]
        public fixed char SymbolicLink[1];

        [FieldOffset(8)]
        public Guid EventGuid;

        [FieldOffset(24)]
        public int NameOffset;

        [FieldOffset(28)]
        public int DataSize;

        [FieldOffset(32)]
        public fixed byte Data[1];

        [FieldOffset(8)]
        public fixed char InstanceId[1];
    }
}
