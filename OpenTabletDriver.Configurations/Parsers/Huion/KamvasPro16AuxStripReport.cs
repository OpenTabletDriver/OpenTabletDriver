using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Wheel;

namespace OpenTabletDriver.Configurations.Parsers.Huion;

/// <summary>
/// Auxiliary report for the Huion Kamvas Pro 16 touch bar.
/// <para/>
/// Byte 4 carries the physical auxiliary button bitmask; byte 5 carries the touch bar's absolute
/// position (<c>0</c> = released, <c>1</c>..<c>7</c> = the seven strip segments). The position is
/// exposed as an absolute wheel so it can be bound to scroll.
/// </summary>
public struct KamvasPro16AuxStripReport : IAuxReport, IAbsoluteWheelReport
{
    public KamvasPro16AuxStripReport(byte[] report)
    {
        Raw = report;

        // Physical auxiliary buttons live on byte 4 (same reading as UCLogicAuxRelWheelReport).
        AuxButtons =
        [
            report[4].IsBitSet(0),
            report[4].IsBitSet(1),
            report[4].IsBitSet(2),
            report[4].IsBitSet(3),
            report[4].IsBitSet(4),
            report[4].IsBitSet(5),
            report[4].IsBitSet(6),
            report[4].IsBitSet(7),
        ];

        // Byte 5 is the absolute strip position: 0 = released -> null; 1-7 -> segments 0-6.
        // Mirrors the mapping used by HuionWheelReport.
        AnalogPositions = [report[5] != 0 ? report[5] - 1u : null];
    }

    public bool[] AuxButtons { set; get; }
    public byte[] Raw { set; get; }
    public uint?[] AnalogPositions { get; set; }
}
