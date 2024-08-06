using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.XP_Pen
{

    public struct XP_PenBatteryReport : IBatteryReport
    {
        public XP_PenBatteryReport(byte[] report)
        {
            Raw = report;
            ChargePercent = report[3];
            PluggedIn = report[4].IsBitSet(0);
        }

        public bool PluggedIn { set; get; }
        public uint ChargePercent { set; get; }
        public byte[] Raw { set; get; }
    }
}
