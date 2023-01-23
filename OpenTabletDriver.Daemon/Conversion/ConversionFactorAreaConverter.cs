using OpenTabletDriver.Attributes;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Daemon.Conversion
{
    [PluginName("Wacom, VEIKK")]
    public class ConversionFactorAreaConverter : IAreaConverter
    {
        public virtual DeviceVendor Vendor => DeviceVendor.Wacom & DeviceVendor.VEIKK;

        public string Top => "Top";
        public string Left => "Left";
        public string Bottom => "Bottom";
        public string Right => "Right";

        private static float GetConversionFactor(TabletConfiguration tablet)
        {
            var digitizer = tablet.Specifications.Digitizer!;
            return digitizer.MaxX / digitizer.Width;
        }

        public AngledArea Convert(TabletConfiguration tablet, float top, float left, float bottom, float right)
        {
            var conversionFactor = GetConversionFactor(tablet);

            var width = (right - left) / conversionFactor;
            var height = (bottom - top) / conversionFactor;
            var offsetX = width / 2 + left / conversionFactor;
            var offsetY = height / 2 + top / conversionFactor;

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
