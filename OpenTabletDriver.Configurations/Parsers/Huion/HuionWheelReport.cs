using OpenTabletDriver.Plugin.Tablet.Wheel;

namespace OpenTabletDriver.Configurations.Parsers.Huion;

public class HuionWheelReport : IWheelButtonReport, IAbsoluteWheelReport
{
    private const uint WHEEL_STEPS = 12;
    private const double HALF_WHEEL_STEPS = WHEEL_STEPS / 2d;
    private const double TREE_HALF_WHEEL_STEPS = HALF_WHEEL_STEPS * 3;

    public HuionWheelReport(byte[] data, ref uint? prevWheelPosition)
    {
        Raw = data;
        var wheelData = data[5];

        if (wheelData == 0)
            Position = null;
        else
        {
            Position = wheelData - 1u;
            Delta = CalculateDelta(prevWheelPosition, Position);
        }
    }

    public byte[] Raw { get; set; }
    public uint? Position { get; set; }
    public int? Delta { get; set; }
    public bool[] WheelButtons { get; set; }

    private static int? CalculateDelta(uint? from, uint? to)
    {
        return (int?)(((to - from + TREE_HALF_WHEEL_STEPS) % WHEEL_STEPS) - HALF_WHEEL_STEPS);
    }
}
