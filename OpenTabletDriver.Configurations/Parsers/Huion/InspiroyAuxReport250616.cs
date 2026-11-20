using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Wheel;

namespace OpenTabletDriver.Configurations.Parsers.Huion;

/// <summary>
/// Appears at least on the Huion Inspiroy M (H951P) with device string <c>HUION_T254_250616</c>
/// </summary>
/// <param name="data"></param>
public struct InspiroyAuxReport250616(byte[] data) : IAuxReport, IRelativeWheelReport
{
    //

    public byte[] Raw { get; set; } = data;
    public bool[] AuxButtons { get; set; } = ParseButtonReport(data);
    public int[] AnalogDeltas { get; set; } = [ParseWheelReport(data)];
    static int ParseWheelReport(byte[] bytes) => (sbyte)bytes[6];

    static bool[] ParseButtonReport(byte[] data) =>
    [
        // top four buttons above the wheel
        data[4].IsBitSet(3),
        data[4].IsBitSet(4),
        data[4].IsBitSet(5),
        data[4].IsBitSet(6),

        // bottom four buttons below the wheel
        data[4].IsBitSet(7),
        data[5].IsBitSet(0),
        data[5].IsBitSet(1),
        data[5].IsBitSet(2),

        // group buttons
        data[4].IsBitSet(0),
        data[4].IsBitSet(1),
        data[4].IsBitSet(2),
    ];
}
