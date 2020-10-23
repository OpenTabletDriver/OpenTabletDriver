namespace OpenTabletDriver.Plugin.Tablet
{
    public interface IAuxReport : IDeviceReport
    {
        bool[] AuxButtons { get; }
        int AuxWheel { get; }
    }
}