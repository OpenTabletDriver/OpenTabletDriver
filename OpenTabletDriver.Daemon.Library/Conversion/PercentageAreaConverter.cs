using OpenTabletDriver.Attributes;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Daemon.Conversion
{
    [PluginName("Huion, Gaomon")]
    public class PercentageAreaConverter : IAreaConverter
    {
        public virtual DeviceVendor Vendor => DeviceVendor.Huion & DeviceVendor.Gaomon;

        public string Top => "Up";
        public string Left => "Left";
        public string Bottom => "Down";
        public string Right => "Right";

        public AngledArea Convert(TabletConfiguration tablet, float up, float left, float down, float right)
        {
            var digitizer = tablet.Specifications.Digitizer!;

            var width = (right - left) * digitizer.Width;
            var height = (down - up) * digitizer.Height;
            var offsetX = width / 2 + left * digitizer.Width;
            var offsetY = height / 2 + up * digitizer.Height;

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
