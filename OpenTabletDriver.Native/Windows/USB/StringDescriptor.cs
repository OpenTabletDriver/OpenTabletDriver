using System;
using System.IO;
using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.Windows.USB
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public readonly struct StringDescriptor
    {
        public readonly byte bLength;

        public readonly DescriptorType bDescriptorType;

        public readonly char bString;

        public const int MaxSize = 255 + 2;

        public unsafe static string GetString(StringDescriptor* descriptor)
        {
            if (descriptor->bDescriptorType != DescriptorType.StringDescriptor && descriptor->bLength != 0)
                throw new IOException($"Invalid descriptor type '{descriptor->bDescriptorType}'. Expected '{DescriptorType.StringDescriptor}'");

            if (descriptor->bLength == 0)
                return "";

            var unicodeLength = (descriptor->bLength - 1) / 2;
            var deviceString = new Span<char>(&descriptor->bString, unicodeLength).ToString();
            return deviceString;
        }

        public unsafe static byte[] GetRaw(StringDescriptor* descriptor)
        {
            return new ReadOnlySpan<byte>(descriptor, descriptor->bLength).ToArray();
        }
    }
}
