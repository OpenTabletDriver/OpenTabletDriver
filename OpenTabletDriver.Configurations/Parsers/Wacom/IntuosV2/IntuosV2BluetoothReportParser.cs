using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.IntuosV2
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public class IntuosV2BluetoothReportParser : IReportParser<IDeviceReport>
    {
        private const int SubPacketSize = 8;
        private const int SubPacketStart = 1;
        private const int MaxSubPackets = 4;

        public IDeviceReport Parse(byte[] data)
        {
            if (data[0] != 0x81)
                return new DeviceReport(data);

            // Find the last non-empty sub-packet
            int lastOffset = -1;
            for (int i = 0; i < MaxSubPackets; i++)
            {
                int offset = SubPacketStart + i * SubPacketSize;
                if (offset + SubPacketSize > data.Length)
                    break;

                if (data[offset] == 0x00)
                    break;

                lastOffset = offset;
            }

            if (lastOffset < 0)
                return new DeviceReport(data);

            var status = data[lastOffset];

            // 0x80 = pen leaving proximity
            if (status == 0x80)
                return new DeviceReport(data);

            // bit 5 set = pen in proximity
            if (status.IsBitSet(5))
                return new IntuosV2BluetoothReport(data, lastOffset);

            return new DeviceReport(data);
        }
    }
}
