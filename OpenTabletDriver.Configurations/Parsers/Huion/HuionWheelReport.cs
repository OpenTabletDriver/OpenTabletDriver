using System;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Huion;

public class HuionWheelReport : IAbsoluteWheelReport
{
    public HuionWheelReport(byte[] data)
    {
        Raw = data;
        var wheelData = data[5];

        if (wheelData == 0)
        {
            WheelPosition = null;
        }
        else
        {

            WheelPosition = wheelData - 1;
        }
    }

    public byte[] Raw { get; set; }
    public int? WheelPosition { get; set; }
}
