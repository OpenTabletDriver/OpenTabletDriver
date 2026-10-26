using System;
using CommunityToolkit.HighPerformance.Buffers;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.IntuosV1
{
    public static class IntuosV1AuxReport
    {
        public static bool Parse(scoped in ReadOnlySpan<byte> report, scoped ref DeviceReport deviceReport)
        {
            var auxByte = report[4];
            deviceReport.AuxButtons = MemoryOwner<bool>.Allocate(8);
            var aux = deviceReport.AuxButtons.Span;

            aux[0] = auxByte.IsBitSet(0);
            aux[1] = auxByte.IsBitSet(1);
            aux[2] = auxByte.IsBitSet(2);
            aux[3] = auxByte.IsBitSet(3);
            aux[4] = auxByte.IsBitSet(4);
            aux[5] = auxByte.IsBitSet(5);
            aux[6] = auxByte.IsBitSet(6);
            aux[7] = auxByte.IsBitSet(7);

            return true;
        }
    }
}
