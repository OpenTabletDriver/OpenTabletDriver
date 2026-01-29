using OpenTabletDriver.Plugin.Tablet.Wheel;

namespace OpenTabletDriver.Configurations.Parsers.Huion;

public class KamvasRelWheelReport : IRelativeWheelReport, IMultiRelativeWheelReport
{
    public KamvasRelWheelReport(byte[] data)
    {
        Raw = data;

        // Byte 3 identifies which wheel generated the report (1 or 2)
        // Byte 5 identifies the direction (0x1 = up/clockwise, 0x2 = down/counter-clockwise)
        int wheelId = data[3];

        // Convert 1-based wheel ID from hardware to 0-based index
        WheelIndex = wheelId > 0 ? wheelId - 1 : 0;

        if (data[5] == 0x1)
            Delta = 1;
        else if (data[5] == 0x2)
            Delta = -1;
    }

    public byte[] Raw { get; set; }
    public int? Delta { get; set; }

    /// <summary>
    /// The index of the wheel that generated this report (0-based).
    /// Wheel 1 = index 0, Wheel 2 = index 1.
    /// </summary>
    public int WheelIndex { get; }
}
