namespace OpenTabletDriver.Plugin.Tablet
{
    public interface IEraserReport : IDeviceReport
    {
        bool Eraser { set; get; }
    }
}
