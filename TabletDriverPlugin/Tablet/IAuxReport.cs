namespace TabletDriverPlugin.Tablet
{
    public interface IAuxReport : IDeviceReport
    {
        bool[] AuxButtons { get; }
    }
}