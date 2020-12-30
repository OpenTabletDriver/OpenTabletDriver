using System.Numerics;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Conversion
{
    [PluginName("Huion")]
    public class PercentageAreaConverter : IAreaConverter
    {
        public virtual DeviceVendor Vendor => DeviceVendor.Huion;

        public Area Convert(TabletState tablet, double left, double top, double right, double bottom)
        {
            var digitizer = tablet.Digitizer;

            var width = (right - left) * digitizer.Width;
            var height = (bottom - top) * digitizer.Height;
            var offsetX = (width / 2) + (left * digitizer.Width);
            var offsetY = (height / 2) + (top * digitizer.Height);

            return new Area((float)width, (float)height, new Vector2((float)offsetX, (float)offsetY), 0f);
        }
    }
}