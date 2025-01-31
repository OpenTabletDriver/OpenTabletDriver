using OpenTabletDriver.Plugin.Tablet.Wheel;

namespace OpenTabletDriver.Configurations.Parsers.Huion;

public class HuionWheelReport : IAbsoluteWheelReport
{
    public HuionWheelReport(byte[] data)
    {
        Raw = data;
        var wheelData = data[5];

        if (wheelData == 0)
            Position = null;
        else
            Position = wheelData - 1u;
    }

    public byte[] Raw { get; set; }
    public uint? Position { get; set; }
    public bool[] WheelButtons { get; set; }
}
