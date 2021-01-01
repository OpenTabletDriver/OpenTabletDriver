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

        public const float XP_PEN_AREA_CONSTANT = 3.937f;

        public Area Convert(TabletState tablet, double left, double top, double right, double bottom)
        {
            double conversionFactor = XP_PEN_AREA_CONSTANT;
            var width = top / conversionFactor;
            var height = left / conversionFactor;
            var offsetX = (width / 2) + (bottom / conversionFactor);
            var offsetY = (height / 2) + (right / conversionFactor);

            return new Area((float)width, (float)height, new Vector2((float)offsetX, (float)offsetY), 0f);
        }
    }
}