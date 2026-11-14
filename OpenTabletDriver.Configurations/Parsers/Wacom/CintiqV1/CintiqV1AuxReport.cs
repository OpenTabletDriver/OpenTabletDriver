using System;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Wheel;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.CintiqV1
{
    public struct CintiqV1AuxReport : IAuxReport, IAbsoluteWheelReport
    {
        public CintiqV1AuxReport(byte[] report,
                                 bool[] topButtons,
                                 uint? firstWheelByte = null,
                                 uint? secondWheelByte = null)
        {
            Raw = report;

            // TODO: Handle touch strips.
            var leftButtons = report[6];
            var rightButtons = report[8];

            AuxButtons =
            [
                leftButtons.IsBitSet(0),
                leftButtons.IsBitSet(1),
                leftButtons.IsBitSet(2),
                leftButtons.IsBitSet(3),
                leftButtons.IsBitSet(4),
                leftButtons.IsBitSet(5),
                leftButtons.IsBitSet(6),
                leftButtons.IsBitSet(7),
                rightButtons.IsBitSet(0),
                rightButtons.IsBitSet(1),
                rightButtons.IsBitSet(2),
                rightButtons.IsBitSet(3),
                rightButtons.IsBitSet(4),
                rightButtons.IsBitSet(5),
                rightButtons.IsBitSet(6),
                rightButtons.IsBitSet(7),
                false, // i-button
                false, // wrench-button
                false, // keyboard-button
            ];

            var topButtonsCount = Math.Min(topButtons.Length, 3);

            for (int i = 0; i < topButtonsCount; i++)
                AuxButtons[i + 16] = topButtons[i];

            AnalogPositions = [null, null];

            // GetValueOrDefault is somehow faster than x.Value by more than 50 nanos somehow
            if (firstWheelByte.HasValue)
                AnalogPositions[0] = GetRingPosition(report[firstWheelByte.GetValueOrDefault()]);
            if (secondWheelByte.HasValue)
                AnalogPositions[1] = GetRingPosition(report[secondWheelByte.GetValueOrDefault()]);
        }

        private static uint? GetRingPosition(byte ring)
            => ring.IsBitSet(7) ? (uint)(ring & 0x7F) : null;

        public byte[] Raw { set; get; }
        public bool[] AuxButtons { set; get; }
        public uint?[] AnalogPositions { set; get; }
    }
}
