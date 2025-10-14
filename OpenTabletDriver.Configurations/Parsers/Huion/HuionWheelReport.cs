using OpenTabletDriver.Plugin.Tablet.Wheel;

namespace OpenTabletDriver.Configurations.Parsers.Huion;

public class HuionWheelReport : IAbsoluteWheelsReport
{
    public HuionWheelReport(byte[] data)
    {
        Raw = data;
        var wheelData = data[5];

        var WheelsPosition = new uint?[1];

        if (wheelData != 0)
            WheelsPosition[0] = wheelData - 1u;
    }

    public byte[] Raw { get; set; }
    public uint?[] WheelsPosition { set; get; }
}
