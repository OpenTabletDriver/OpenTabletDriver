using OpenTabletDriver.Attributes;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Daemon.Conversion
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

        public AngledArea Convert(TabletConfiguration tablet, float w, float h, float x, float y)
        {
            var width = w / XP_PEN_AREA_CONSTANT;
            var height = h / XP_PEN_AREA_CONSTANT;
            var offsetX = width / 2 + x / XP_PEN_AREA_CONSTANT;
            var offsetY = height / 2 + y / XP_PEN_AREA_CONSTANT;

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
