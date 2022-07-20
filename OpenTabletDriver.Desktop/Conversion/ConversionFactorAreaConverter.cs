using OpenTabletDriver.Attributes;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Desktop.Conversion
{
    [PluginName("Wacom, VEIKK")]
    public class ConversionFactorAreaConverter : IAreaConverter
    {
        public virtual DeviceVendor Vendor => DeviceVendor.Wacom & DeviceVendor.VEIKK;

        public string Top => "Top";
        public string Left => "Left";
        public string Bottom => "Bottom";
        public string Right => "Right";

        private static double GetConversionFactor(InputDevice tablet)
        {
            var digitizer = tablet.Configuration.Specifications.Digitizer;
            return digitizer.MaxX / digitizer.Width;
        }

        public AngledArea Convert(InputDevice tablet, double top, double left, double bottom, double right)
        {
            var conversionFactor = GetConversionFactor(tablet);

            var width = (float) ((right - left) / conversionFactor);
            var height = (float) ((bottom - top) / conversionFactor);
            var offsetX = (float) (width / 2 + left / conversionFactor);
            var offsetY = (float) (height / 2 + top / conversionFactor);

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
