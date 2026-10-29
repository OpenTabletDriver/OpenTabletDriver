using OpenTabletDriver.Plugin.Tablet.Wheel;

namespace OpenTabletDriver.Configurations.Parsers.Huion;

public struct HuionWheelReport : IAbsoluteWheelReport
{
    public HuionWheelReport(byte[] data)
    {
        Raw = data;
        var wheelData = data[5];

        AnalogPositions = [wheelData != 0 ? wheelData - 1u : null];
    }

    public byte[] Raw { get; set; }
    public uint?[] AnalogPositions { get; set; }
}
