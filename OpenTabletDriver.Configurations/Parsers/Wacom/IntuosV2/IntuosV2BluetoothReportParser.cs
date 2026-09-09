using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.IntuosV2
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public class IntuosV2BluetoothReportParser : IReportParser<IDeviceReport>
    {
        private const int MinimumReportLength = 46;
        private const int SubPacketSize = 8;
        private const int SubPacketStart = 1;
        private const int MaxSubPackets = 4;

        private Vector2 _lastPosition = Vector2.Zero;

        public IDeviceReport Parse(byte[] data)
        {
            if (data.Length < MinimumReportLength || data[0] != 0x81)
                return new DeviceReport(data);

            int lastOffset = -1;
            for (int i = 0; i < MaxSubPackets; i++)
            {
                int offset = SubPacketStart + i * SubPacketSize;
                if (data[offset].IsBitSet(7))
                    lastOffset = offset;
            }

            if (lastOffset < 0)
                return new IntuosV2BluetoothAuxReport(data);

            var status = data[lastOffset];
            if (!status.IsBitSet(6))
                return new OutOfRangeReport(data);

            var report = new IntuosV2BluetoothReport(data, lastOffset);
            if (status.IsBitSet(5))
                _lastPosition = report.Position;
            else
                report.Position = _lastPosition;

            return report;
        }
    }
}
