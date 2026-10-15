using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Wheel;

namespace OpenTabletDriver.Configurations.Parsers.Silvercrest;

public struct SilvercrestWheelReport(byte[] data) : IAuxReport, IRelativeWheelReport, IWheelButtonReport
{
    public byte[] Raw { get; set; } = data;
    public bool[] AuxButtons { get; set; } = [
        data[1].IsBitSet(2), // Scroll
        data[1].IsBitSet(3), // Zoom
        data[1].IsBitSet(4), // Volume
    ];
    public int[] AnalogDeltas { get; set; } = [
        (sbyte)data[2] // Report FF left, 0x01 right
    ];
    public bool[][] WheelButtons { get; set; } = [
        [
            data[3] == 0x01
        ]
    ];
}
