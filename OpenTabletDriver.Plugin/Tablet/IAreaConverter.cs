namespace OpenTabletDriver.Plugin.Tablet
{
    public interface IAreaConverter
    {
        DeviceVendor Vendor { get; }

        Area Convert(TabletState tablet, double left, double top, double right, double bottom);
    }
}