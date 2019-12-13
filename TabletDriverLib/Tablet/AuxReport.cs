namespace TabletDriverLib.Tablet
{
    public struct AuxReport : IAuxReport
    {
        public AuxReport(byte[] report)
        {
            Raw = report;
            // TODO: Add aux button indicators
            AuxButtons = null;
        }

        public byte[] Raw { private set; get; }
        public bool[] AuxButtons { private set; get; }
    }
}