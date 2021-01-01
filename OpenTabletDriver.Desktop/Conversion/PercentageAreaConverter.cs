using System.Numerics;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Conversion
{
    [PluginName("Huion, Gaomon")]
    public class PercentageAreaConverter : IAreaConverter
    {
        public virtual DeviceVendor Vendor => DeviceVendor.Huion & DeviceVendor.Gaomon;

        public string[] Label => new string[] { "Up", "Left", "Down", "Right" };

        public Area Convert(TabletState tablet, double up, double left, double down, double right)
        {
            var digitizer = tablet.Digitizer;

            var width = (right - left) * digitizer.Width;
            var height = (down - up) * digitizer.Height;
            var offsetX = (width / 2) + (left * digitizer.Width);
            var offsetY = (height / 2) + (up * digitizer.Height);

            return new Area((float)width, (float)height, new Vector2((float)offsetX, (float)offsetY), 0f);
        }
    }
}