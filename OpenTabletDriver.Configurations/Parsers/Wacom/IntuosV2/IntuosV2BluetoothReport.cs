using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.IntuosV2
{
    public struct IntuosV2BluetoothReport : ITabletReport, IProximityReport, IEraserReport, IAuxReport
    {
        public IntuosV2BluetoothReport(byte[] report, int offset)
        {
            Raw = report;

            var penByte = report[offset];

            Position = new Vector2
            {
                X = Unsafe.ReadUnaligned<ushort>(ref report[offset + 1]),
                Y = Unsafe.ReadUnaligned<ushort>(ref report[offset + 3])
            };
            Pressure = Unsafe.ReadUnaligned<ushort>(ref report[offset + 5]);

            Eraser = penByte.IsBitSet(4);
            PenButtons =
            [
                penByte.IsBitSet(1),
                penByte.IsBitSet(2),
            ];
            NearProximity = penByte.IsBitSet(5);
            HoverDistance = report[offset + 7];

            var auxByte = report[44];
            AuxButtons =
            [
                auxByte.IsBitSet(0),
                auxByte.IsBitSet(1),
                auxByte.IsBitSet(2),
                auxByte.IsBitSet(3),
                auxByte.IsBitSet(4),
                auxByte.IsBitSet(5),
                auxByte.IsBitSet(6),
                auxByte.IsBitSet(7),
            ];
        }

        public byte[] Raw { set; get; }
        public Vector2 Position { set; get; }
        public uint Pressure { set; get; }
        public bool Eraser { set; get; }
        public bool[] PenButtons { set; get; }
        public bool NearProximity { set; get; }
        public uint HoverDistance { set; get; }
        public bool[] AuxButtons { set; get; }
    }
}
