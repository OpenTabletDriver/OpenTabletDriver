namespace TabletDriverLib.Tablet
{
    public interface IAuxReport : IDeviceReport
    {
        bool[] AuxButtons { get; }
    }
}