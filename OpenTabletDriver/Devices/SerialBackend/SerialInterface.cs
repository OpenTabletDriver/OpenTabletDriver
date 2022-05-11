using System;

namespace OpenTabletDriver.Devices.SerialBackend
{
    public class SerialInterface : IDeviceEndpoint
    {
        public unsafe SerialInterface(string devicePath)
        {
            DevicePath = devicePath;
        }

        internal int InterfaceNum { get; private set; }
        internal byte? InputPipe { get; private set; }
        internal byte? OutputPipe { get; private set; }

        public int ProductID { get; private set; }

        public int VendorID { get; private set; }

        public int InputReportLength { get; private set; }

        public int OutputReportLength { get; private set; }

        public int FeatureReportLength => 0; // requires parsing report descriptor to determine feature report length

        public string Manufacturer { get; private set; }

        public string ProductName { get; private set; }

        public string FriendlyName => ProductName;

        public string SerialNumber { get; private set; }

        public string DevicePath { get; }

        public bool CanOpen => true;

        public IDeviceEndpointStream Open()
        {
            return new SerialInterfaceStream(new WeakReference<SerialInterface>(this));
        }

        public string GetDeviceString(byte index) => "null";
    }
}
