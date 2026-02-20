using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.Windows.USB
{
    /// <summary>
    /// <a href="https://learn.microsoft.com/en-us/windows-hardware/drivers/ddi/usbspec/ns-usbspec-_usb_device_descriptor">Microsoft usbspec.h</a>
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct DeviceDescriptor
    {
        public readonly DescriptorHeader Header;

        /// <summary>
        /// Identifies the version of the USB specification that this descriptor structure complies with. This value is a binary-coded decimal number.
        /// </summary>
        public readonly ushort bcdUSB;

        /// <summary>
        /// Specifies the class code of the device as assigned by the USB specification group.
        /// </summary>
        public readonly byte bDeviceClass;

        /// <summary>
        /// Specifies the subclass code of the device as assigned by the USB specification group.
        /// </summary>
        public readonly byte bDeviceSubClass;

        /// <summary>
        /// Specifies the protocol code of the device as assigned by the USB specification group.
        /// </summary>
        public readonly byte bDeviceProtocol;

        /// <summary>
        /// Specifies the maximum packet size, in bytes, for endpoint zero of the device. The value must be set to 8, 16, 32, or 64.
        /// </summary>
        public readonly byte bMaxPacketSize0;

        /// <summary>
        /// Specifies the vendor identifier for the device as assigned by the USB specification committee.
        /// </summary>
        public readonly ushort idVendor;

        /// <summary>
        /// Specifies the product identifier. This value is assigned by the manufacturer and is device-specific.
        /// </summary>
        public readonly ushort idProduct;

        /// <summary>
        /// Identifies the version of the device. This value is a binary-coded decimal number.
        /// </summary>
        public readonly ushort bcdDevice;

        /// <summary>
        /// Specifies a device-defined index of the string descriptor that provides a string containing the name of the manufacturer of this device.
        /// </summary>
        public readonly byte iManufacturer;

        /// <summary>
        /// Specifies a device-defined index of the string descriptor that provides a string that contains a description of the device.
        /// </summary>
        public readonly byte iProduct;

        /// <summary>
        /// Specifies a device-defined index of the string descriptor that provides a string that contains a manufacturer-determined serial number for the device.
        /// </summary>
        public readonly byte iSerialNumber;

        /// <summary>
        /// Specifies the total number of possible configurations for the device.
        /// </summary>
        public readonly byte bNumConfigurations;
    }
}
