using OpenTabletDriver.Attributes;
using OpenTabletDriver.Tablet;

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

        public AngledArea Convert(InputDevice tablet, double w, double h, double x, double y)
        {
            double conversionFactor = XP_PEN_AREA_CONSTANT;
            var width = (float) (w / conversionFactor);
            var height = (float) (h / conversionFactor);
            var offsetX = (float) (width / 2 + x / conversionFactor);
            var offsetY = (float) (height / 2 + y / conversionFactor);

            return new AngledArea
            {
                Width = width,
                Height = height,
                XPosition = offsetX,
                YPosition = offsetY,
                Rotation = 0
            };
        }
    }
}
