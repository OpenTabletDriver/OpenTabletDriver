using System;
using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.Windows.CM
{
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct CM_NOTIFY_FILTER
    {
        private const int MAX_DEVICE_ID_LEN = 200;

        [FieldOffset(0)]
        public int cbSize;

        [FieldOffset(4)]
        public CM_NOTIFY_FILTER_FLAG Flags;

        [FieldOffset(8)]
        public CM_NOTIFY_FILTER_TYPE FilterType;

        [FieldOffset(12)]
        public uint Reserved;

        [FieldOffset(16)]
        public fixed char InstanceId[MAX_DEVICE_ID_LEN];

        [FieldOffset(16)]
        public IntPtr hTarget;

        [FieldOffset(16)]
        public Guid ClassGuid;

        public static CM_NOTIFY_FILTER AllInterfaces
        {
            get
            {
                var filter = Create();
                filter.Flags = CM_NOTIFY_FILTER_FLAG.ALL_DEVICE_INSTANCES;
                filter.FilterType = CM_NOTIFY_FILTER_TYPE.DEVICEINSTANCE;
                return filter;
            }
        }

        public static CM_NOTIFY_FILTER AllDevices
        {
            get
            {
                var filter = Create();
                filter.Flags = CM_NOTIFY_FILTER_FLAG.ALL_INTERFACE_CLASSES;
                filter.FilterType = CM_NOTIFY_FILTER_TYPE.DEVICEINTERFACE;
                return filter;
            }
        }

        public static CM_NOTIFY_FILTER Create()
        {
            return new CM_NOTIFY_FILTER()
            {
                cbSize = Marshal.SizeOf<CM_NOTIFY_FILTER>(),
            };
        }

        public static CM_NOTIFY_FILTER Create(Guid classGuid)
        {
            return new CM_NOTIFY_FILTER()
            {
                cbSize = Marshal.SizeOf<CM_NOTIFY_FILTER>(),
                ClassGuid = classGuid,
                FilterType = CM_NOTIFY_FILTER_TYPE.DEVICEINTERFACE,
            };
        }

        public static CM_NOTIFY_FILTER Create(IntPtr target)
        {
            return new CM_NOTIFY_FILTER()
            {
                cbSize = Marshal.SizeOf<CM_NOTIFY_FILTER>(),
                hTarget = target,
                FilterType = CM_NOTIFY_FILTER_TYPE.DEVICEHANDLE,
            };
        }

        public static CM_NOTIFY_FILTER Create(string instanceId)
        {
            if (instanceId == null)
                throw new ArgumentNullException(nameof(instanceId));

            if (instanceId.Length > MAX_DEVICE_ID_LEN)
                throw new ArgumentException("The length of instanceId cannot exceed " + nameof(MAX_DEVICE_ID_LEN), nameof(instanceId));

            var filter = new CM_NOTIFY_FILTER()
            {
                cbSize = Marshal.SizeOf<CM_NOTIFY_FILTER>(),
                FilterType = CM_NOTIFY_FILTER_TYPE.DEVICEINSTANCE,
            };

            fixed (char* pInstanceId = instanceId)
            {
                int cbSource = instanceId.Length * 2; // char to byte length
                int cbTarget = MAX_DEVICE_ID_LEN * 2; // char to byte length
                Buffer.MemoryCopy(pInstanceId, filter.InstanceId, cbTarget, cbSource);
            }

            return filter;
        }
    }
}
