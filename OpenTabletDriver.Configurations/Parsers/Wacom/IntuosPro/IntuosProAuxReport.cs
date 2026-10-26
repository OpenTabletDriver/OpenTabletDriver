using System;
using CommunityToolkit.HighPerformance.Buffers;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.IntuosPro
{
    public struct IntuosProAuxReport
    {
        public static bool Parse(scoped ReadOnlySpan<byte> report, ref DeviceReport deviceReport)
        {
            var touchWheelButtonByte = report[3];
            var auxByte = report[4];

            deviceReport.AuxButtons = MemoryOwner<bool>.Allocate(8);
            var auxButtonsSpan = deviceReport.AuxButtons.Span;
            auxButtonsSpan[0] = auxByte.IsBitSet(0);
            auxButtonsSpan[1] = auxByte.IsBitSet(1);
            auxButtonsSpan[2] = auxByte.IsBitSet(2);
            auxButtonsSpan[3] = auxByte.IsBitSet(3);
            auxButtonsSpan[4] = auxByte.IsBitSet(4);
            auxButtonsSpan[5] = auxByte.IsBitSet(5);
            auxButtonsSpan[6] = auxByte.IsBitSet(6);
            auxButtonsSpan[7] = auxByte.IsBitSet(7);

            var wheelByte = report[2];

            // Wheel Start at Position zero (0x80) and Provides a value between 0x80 & 0xC7 on PTH-x50 & PTH-x51
            deviceReport.WheelPositions = MemoryOwner<uint?>.Allocate(1);
            deviceReport.WheelPositions.Span[0] = wheelByte.IsBitSet(7) ? (uint)(wheelByte & 0x7f) : null;

            deviceReport.WheelButtons = MemoryOwner<MemoryOwner<bool>>.Allocate(1);
            deviceReport.WheelButtons.Span[0] = MemoryOwner<bool>.Allocate(1);
            deviceReport.WheelButtons.Span[0].Span[0] = touchWheelButtonByte.IsBitSet(0);

            return true;
        }
    }
}
