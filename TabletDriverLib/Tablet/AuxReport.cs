using System;

namespace TabletDriverLib.Tablet
{
    public struct AuxReport : IAuxReport
    {
        public AuxReport(byte[] report)
        {
            Raw = report;
            AuxButtons = new bool[]
            {
                (report[3] & (1 << 0)) != 0,
                (report[3] & (1 << 1)) != 0,
                (report[3] & (1 << 2)) != 0,
                (report[3] & (1 << 3)) != 0
            };
        }

        public byte[] Raw { private set; get; }
        public bool[] AuxButtons { private set; get; }

        public override string ToString() => ToString(Driver.RawReports);

        public string ToString(bool isRaw)
        {
            if (isRaw)
                return BitConverter.ToString(Raw);
            else
                return $"AuxButtons:[{String.Join(" ", AuxButtons)}]";
        }
    }
}