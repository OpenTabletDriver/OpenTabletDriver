using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Supernote // Create a new namespace for Supernote
{
    public struct SupernoteNomadReport : ITabletReport, ITiltReport // Implement necessary interfaces
    {
        // Constructor takes the raw byte array
        public SupernoteNomadReport(byte[] report)
        {
            Raw = report;
            Tip = report[1].IsBitSet(0);
            Proximity = report[1].IsBitSet(1); // Assuming Proximity is still valid

            if (!Tip)
            {
                Pressure = 0;
            }
            else
            {
                uint rawPressure = report[2];
                // Subtract the offset (C0 hex = 192 dec)
                uint adjustedPressure = (uint)Math.Max(0, (int)rawPressure - 192);
                // Scale the adjusted range (0-63) to the target range (0-4095)
                // Scaling factor = TargetMax / (HardwareMax - HardwareMin) = 4095.0 / (255.0 - 192.0) = 4095.0 / 63.0
                const double scaleFactor = 4095.0 / 63.0;
                Pressure = (uint)Math.Min(4095, Math.Round(adjustedPressure * scaleFactor)); // Apply scaling and clamp to 0-4095
            }
            Position = new Vector2
            {
                X = Unsafe.ReadUnaligned<ushort>(ref report[3]),
                Y = Unsafe.ReadUnaligned<ushort>(ref report[5])
            };
            Tilt = new Vector2
            {
                X = Unsafe.ReadUnaligned<sbyte>(ref report[7]),
                Y = Unsafe.ReadUnaligned<sbyte>(ref report[8])
            };
            PenButtons = System.Array.Empty<bool>();
            Eraser = false;
        }

        public byte[] Raw { get; set; } // Raw report data
        public Vector2 Position { get; set; } // X and Y coordinates
        public uint Pressure { get; set; } // Pressure value
        public bool[] PenButtons { get; set; } // Array of button states
        public bool Eraser { get; set; } // Eraser state
        public bool Tip { get; set; } // Tip switch state
        public bool Proximity { get; set; } // Proximity state
        public Vector2 Tilt { get; set; } // Tilt X and Y coordinates
    }
}
