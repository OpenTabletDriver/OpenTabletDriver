using OpenTabletDriver.Plugin.Tablet.Wheel;

namespace OpenTabletDriver.Configurations.Parsers.Huion;

public class KamvasRelWheelReport : IRelativeWheelReport
{
    public KamvasRelWheelReport(byte[] data)
    {
        if (data == null)
            return;
        Raw = data;

        // TODO: Handle when multi-wheel support is added
        switch (data[3])
        {
            // Wheel 1 moved
            case 1:
                if (data[5] == 0x1)
                    Delta = 1;
                else if (data[5] == 0x2)
                    Delta = -1;
                break;
            // Wheel 2 moved
            case 2:
                if (data[5] == 0x1)
                    Delta = 1;
                else if (data[5] == 0x2)
                    Delta = -1;
                break;
        }
    }

    public byte[] Raw { get; set; }
    public int? Delta { get; set; }
}
