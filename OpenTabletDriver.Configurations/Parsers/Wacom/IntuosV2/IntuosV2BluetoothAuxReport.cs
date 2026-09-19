using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.IntuosV2
{
    public struct IntuosV2BluetoothAuxReport : IAuxReport
    {
        public IntuosV2BluetoothAuxReport(byte[] report)
        {
            Raw = report;
            AuxButtons = IntuosV2BluetoothReport.ParseAuxButtons(report[44]);
        }

        public byte[] Raw { set; get; }
        public bool[] AuxButtons { set; get; }
    }
}
