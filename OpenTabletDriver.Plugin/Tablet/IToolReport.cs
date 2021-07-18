namespace OpenTabletDriver.Plugin.Tablet
{
    public interface IToolReport : IDeviceReport
    {
        ulong Serial { set; get; }
        uint RawToolID { set; get; }
        ToolType Tool { set; get; }
    }
}
