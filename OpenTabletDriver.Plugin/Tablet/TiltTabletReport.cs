using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using CommunityToolkit.HighPerformance.Buffers;

namespace OpenTabletDriver.Plugin.Tablet
{
    public class TiltTabletReport
    {
        public static bool Parse(scoped ReadOnlySpan<byte> raw, ref DeviceReport deviceReport)
            => Parse(raw, ref deviceReport, false, false);

        public static bool Parse(scoped ReadOnlySpan<byte> report, ref DeviceReport deviceReport, bool invertTiltX, bool invertTiltY)
        {
            deviceReport.PenPosition = new Vector2
            {
                X = Unsafe.ReadUnaligned<ushort>(in report[2]),
                Y = Unsafe.ReadUnaligned<ushort>(in report[4])
            };
            deviceReport.Tilt = new Vector2
            {
                X = (sbyte)(invertTiltX ? -1 : 1) * (sbyte)report[10],
                Y = (sbyte)(invertTiltY ? -1 : 1) * (sbyte)report[11]
            };
            deviceReport.PenPressure = Unsafe.ReadUnaligned<ushort>(in report[6]);

            deviceReport.PenButtons = MemoryOwner<bool>.Allocate(3);
            var penButtonSpan = deviceReport.PenButtons.Span;

            var penByte = report[1];
            penButtonSpan[0] = penByte.IsBitSet(1);
            penButtonSpan[1] = penByte.IsBitSet(2);
            penButtonSpan[2] = penByte.IsBitSet(3);

            return true;
        }
    }
}
