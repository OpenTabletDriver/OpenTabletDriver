using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using CommunityToolkit.HighPerformance.Buffers;

namespace OpenTabletDriver.Plugin.Tablet
{
    public class TabletReport
    {
        public static bool Parse(scoped in ReadOnlySpan<byte> report, scoped ref DeviceReport deviceReport)
        {
            deviceReport.PenPosition = new Vector2
            {
                X = Unsafe.ReadUnaligned<ushort>(in report[2]),
                Y = Unsafe.ReadUnaligned<ushort>(in report[4]),
            };

            deviceReport.PenPressure = Unsafe.ReadUnaligned<ushort>(in report[6]);

            //deviceReport.PenButtons = MemoryOwner<bool>.Allocate(3);
            //var penButtonSpan = deviceReport.PenButtons.Span;
            //penButtonSpan[0] = report[1].IsBitSet(1);
            //penButtonSpan[1] = report[1].IsBitSet(2);
            //penButtonSpan[2] = report[1].IsBitSet(3);

            return true;
        }
    }
}
