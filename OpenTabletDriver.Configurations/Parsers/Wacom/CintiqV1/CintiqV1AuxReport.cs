using System;
using System.Buffers.Binary;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.CintiqV1
{
    public struct CintiqV1AuxReport : IAuxReport, ITouchStripReport
    {
        public CintiqV1AuxReport(byte[] report, ref ushort _prevLeftTouchStrip, ref ushort _prevRightTouchStrip)
        {
            Raw = report;

            ushort nextLeftTouchStrip = BinaryPrimitives.ReadUInt16BigEndian(report.AsSpan(1));
            ushort nextRightTouchStrip = BinaryPrimitives.ReadUInt16BigEndian(report.AsSpan(3));
            var leftTouchStripDir = GetTouchStripDirection(_prevLeftTouchStrip, nextLeftTouchStrip);
            var rightTouchStripDir = GetTouchStripDirection(_prevRightTouchStrip, nextRightTouchStrip);
            _prevLeftTouchStrip = nextLeftTouchStrip;
            _prevRightTouchStrip = nextRightTouchStrip;

            TouchStripDirections = new TouchStripDirection[]
            {
                leftTouchStripDir,
                rightTouchStripDir,
            };

            var leftRadialButton = report[5];
            var leftButtons = report[6];
            var rightRadialButton = report[7];
            var rightButtons = report[8];
            var topButtons = report[9];
            AuxButtons = new bool[]
            {
                leftRadialButton.IsBitSet(0),
                leftButtons.IsBitSet(0),
                leftButtons.IsBitSet(1),
                leftButtons.IsBitSet(2),
                leftButtons.IsBitSet(3),
                leftButtons.IsBitSet(4),
                leftButtons.IsBitSet(5),
                leftButtons.IsBitSet(6),
                leftButtons.IsBitSet(7),
                leftButtons.IsBitSet(8),
                rightRadialButton.IsBitSet(0),
                rightButtons.IsBitSet(0),
                rightButtons.IsBitSet(1),
                rightButtons.IsBitSet(2),
                rightButtons.IsBitSet(3),
                rightButtons.IsBitSet(4),
                rightButtons.IsBitSet(5),
                rightButtons.IsBitSet(6),
                rightButtons.IsBitSet(7),
                rightButtons.IsBitSet(8),
                topButtons.IsBitSet(0), // i-button
                topButtons.IsBitSet(1), // wrench-button
            };
        }

        private TouchStripDirection GetTouchStripDirection(ushort prevTouchStrip, ushort nextTouchStrip)
        {
            if (prevTouchStrip == 0 || nextTouchStrip == 0 || prevTouchStrip == nextTouchStrip)
            {
                return TouchStripDirection.None;
            }
            else if (prevTouchStrip < nextTouchStrip)
            {
                return TouchStripDirection.Down;
            }
            else
            {
                return TouchStripDirection.Up;
            }
        }

        public byte[] Raw { set; get; }
        public bool[] AuxButtons { set; get; }
        public TouchStripDirection[] TouchStripDirections { set; get; }
    }
}
