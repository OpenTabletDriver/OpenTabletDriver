namespace OpenTabletDriver.Plugin.Tablet
{
    public interface IAreaConverter
    {
        DeviceVendor Vendor { get; }

        string Top { get; }
        string Left { get; }
        string Bottom { get; }
        string Right { get; }

        Area Convert(TabletState tablet, double top, double left, double bottom, double right);
    }
}