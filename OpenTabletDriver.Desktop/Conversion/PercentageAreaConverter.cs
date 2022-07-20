using OpenTabletDriver.Attributes;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Desktop.Conversion
{
    [PluginName("Huion, Gaomon")]
    public class PercentageAreaConverter : IAreaConverter
    {
        public virtual DeviceVendor Vendor => DeviceVendor.Huion & DeviceVendor.Gaomon;

        public string Top => "Up";
        public string Left => "Left";
        public string Bottom => "Down";
        public string Right => "Right";

        public AngledArea Convert(InputDevice tablet, double up, double left, double down, double right)
        {
            var digitizer = tablet.Configuration.Specifications.Digitizer;

            var width = (float) ((right - left) * digitizer.Width);
            var height = (float) ((down - up) * digitizer.Height);
            var offsetX = (float) (width / 2 + left * digitizer.Width);
            var offsetY = (float) (height / 2 + up * digitizer.Height);

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
