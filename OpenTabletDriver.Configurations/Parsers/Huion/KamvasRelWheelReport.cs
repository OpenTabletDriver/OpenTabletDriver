using OpenTabletDriver.Plugin.Tablet.Wheel;

namespace OpenTabletDriver.Configurations.Parsers.Huion;

public struct KamvasRelWheelReport : IRelativeWheelReport
{
    public KamvasRelWheelReport(byte[] data)
    {
        Raw = data;

        AnalogDeltas = [
            data[3] == 1 ? GetWheelDelta(data[5]) : 0,
            data[3] == 2 ? GetWheelDelta(data[5]) : 0,
        ];
    }

    private static int GetWheelDelta(byte wheelData) =>
        wheelData switch
        {
            0x1 => 1,
            0x2 => -1,
            _ => 0,
        };

    public byte[] Raw { get; set; }
    public int[] AnalogDeltas { get; set; }
}
