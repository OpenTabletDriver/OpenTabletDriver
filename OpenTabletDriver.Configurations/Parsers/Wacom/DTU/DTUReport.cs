using System;
using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.DTU
{
    public struct DTUReport : ITabletReport
    {
        public DTUReport(byte[] report)
        {
            Raw = report;

            Position = new Vector2
            {
                X = BinaryPrimitives.ReadUInt16BigEndian(report.AsSpan(3, 4)),
                Y = BinaryPrimitives.ReadUInt16BigEndian(report.AsSpan(5, 6))
            };
            Pressure = BinaryPrimitives.ReadUInt16BigEndian(report.AsSpan(1, 2)) & (uint)0x0FFFu;

            var penByte = (byte)(report[1] >> 4);
            // Bit 0 = Pen Tip
            // Bit 1 = Pen Button 1
            // Bit 2 = Pen Button 2
            // Bit 3 = Hovering
            PenButtons = new bool[]
            {
                penByte.IsBitSet(1),
                penByte.IsBitSet(2)
            };
        }

        public byte[] Raw { set; get; }
        public Vector2 Position { set; get; }
        public uint Pressure { set; get; }
        public bool[] PenButtons { set; get; }
    }
}
