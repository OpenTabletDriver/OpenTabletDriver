using System.Numerics;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Conversion
{
    [PluginName("Gaomon, 2024 and newer")]
    public class GaomonV2AreaConverter : IAreaConverter
    {
        public virtual DeviceVendor Vendor => DeviceVendor.Gaomon;

        public string Top => "Width";
        public string Left => "Height";
        public string Bottom => "X";
        public string Right => "Y";

        public Area Convert(TabletReference tablet, double iWidth, double iHeight, double x, double y)
        {
            var digitizer = tablet.Properties.Specifications.Digitizer;

            var oWidth = (iWidth / digitizer.MaxX) * digitizer.Width;
            var oHeight = (iHeight / digitizer.MaxY) * digitizer.Height;
            var offsetX = (x / digitizer.MaxX) * digitizer.Width + (oWidth / 2);
            var offsetY = (x / digitizer.MaxY) * digitizer.Height + (oHeight / 2);

            return new Area((float)oWidth, (float)oHeight, new Vector2((float)offsetX, (float)offsetY), 0f);
        }
    }
}
