using OpenTabletDriver.Plugin.Tablet.Wheel;

namespace OpenTabletDriver.Configurations.Parsers.Huion;

public class HuionWheelReport : IWheelButtonReport, IAbsoluteWheelReport
{
    public HuionWheelReport(byte[] data)
    {
        Raw = data;
        var wheelData = data[5];

        if (wheelData == 0)
            Position = null;
        else
            Position = wheelData - 1u;

        WheelButtons = [];
    }

    public byte[] Raw { get; set; }
    public uint? Position { get; set; }
    public bool[] WheelButtons { get; set; }
}
