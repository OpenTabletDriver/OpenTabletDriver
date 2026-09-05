using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.IntuosV2
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public class IntuosV2BluetoothReportParser : IReportParser<IDeviceReport>
    {
        private const int SubPacketSize = 8;
        private const int SubPacketStart = 1;
        private const int MaxSubPackets = 4;

        private Vector2 _lastPosition = Vector2.Zero;

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

            IntuosV2BluetoothReport report;

            if (lastOffset < 0)
            {
                report = new IntuosV2BluetoothReport(data, 0);
            }
            else
            {
                report = new IntuosV2BluetoothReport(data, lastOffset);
            }

            if (!report.NearProximity)
            {
                report.Position = _lastPosition;
            }
            else
            {
                _lastPosition = report.Position;
            }

            return report;
        }
    }
}
