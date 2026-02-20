using OpenTabletDriver.Plugin.Tablet.Wheel;

namespace OpenTabletDriver.Configurations.Parsers.Huion;

public struct HuionWheelReport : IAbsoluteWheelReport
{
    public HuionWheelReport(byte[] data)
    {
        Raw = data;
        var wheelData = data[5];

        if (wheelData != 0)
            Position = wheelData - 1u;
    }

    public byte[] Raw { get; set; }
    public uint? Position { get; set; }
}
