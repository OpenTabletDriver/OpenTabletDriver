using System.Numerics;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Conversion
{
    [PluginName("XP Pen")]
    public class XP_PenDriverAreaConverter : IAreaConverter
    {
        public virtual DeviceVendor Vendor => DeviceVendor.XP_Pen;

        public string Top => "W";
        public string Left => "H";
        public string Bottom => "X";
        public string Right => "Y";

        private const float XP_PEN_AREA_CONSTANT = 3.937f;

        public Area Convert(TabletReference tablet, double w, double h, double x, double y)
        {
            double conversionFactor = XP_PEN_AREA_CONSTANT;
            var width = w / conversionFactor;
            var height = h / conversionFactor;
            var offsetX = (width / 2) + (x / conversionFactor);
            var offsetY = (height / 2) + (y / conversionFactor);

            return new Area((float)width, (float)height, new Vector2((float)offsetX, (float)offsetY), 0f);
        }
    }
}
