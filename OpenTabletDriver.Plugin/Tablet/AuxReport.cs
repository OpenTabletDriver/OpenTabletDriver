using System;
using CommunityToolkit.HighPerformance.Buffers;

namespace OpenTabletDriver.Plugin.Tablet
{
    public struct AuxReport
    {
        public static bool Parse(scoped ReadOnlySpan<byte> report, ref DeviceReport deviceReport)
        {
            var auxByte = report[3];
            deviceReport.AuxButtons = MemoryOwner<bool>.Allocate(4);

            var auxButtons = deviceReport.AuxButtons.Span;
            auxButtons[0] = auxByte.IsBitSet(0);
            auxButtons[1] = auxByte.IsBitSet(1);
            auxButtons[2] = auxByte.IsBitSet(2);
            auxButtons[3] = auxByte.IsBitSet(3);

            return true;
        }
    }
}
