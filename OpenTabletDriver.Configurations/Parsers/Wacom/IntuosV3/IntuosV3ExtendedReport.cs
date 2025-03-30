﻿using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.IntuosV3
{
    public struct IntuosV3ExtendedReport : ITabletReport, IHoverReport, IConfidenceReport, ITiltReport, IEraserReport
    {
        public IntuosV3ExtendedReport(byte[] report)
        {
            Raw = report;

            Position = new Vector2
            {
                X = Unsafe.ReadUnaligned<ushort>(ref report[3]) | report[5] << 16,
                Y = Unsafe.ReadUnaligned<ushort>(ref report[6]) | report[8] << 16
            };

            Tilt = new Vector2
            {
                X = Unsafe.ReadUnaligned<short>(ref report[11]),
                Y = Unsafe.ReadUnaligned<short>(ref report[13])
            };

            Pressure = Unsafe.ReadUnaligned<ushort>(ref report[9]);

            var penByte = report[2];
            PenButtons = new bool[]
            {
                penByte.IsBitSet(1),
                penByte.IsBitSet(2),
                penByte.IsBitSet(3)
            };
            Eraser = report[2].IsBitSet(5);
            HighConfidence = report[2].IsBitSet(6);
            HoverDistance = report[19];
        }

        public byte[] Raw { set; get; }
        public Vector2 Position { set; get; }
        public Vector2 Tilt { set; get; }
        public uint Pressure { set; get; }
        public bool[] PenButtons { set; get; }
        public bool HighConfidence { set; get; }
        public uint HoverDistance { set; get; }
        public bool Eraser { get; set; }
    }
}
