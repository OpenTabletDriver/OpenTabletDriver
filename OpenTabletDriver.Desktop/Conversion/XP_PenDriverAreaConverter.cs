using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Conversion
{
    [PluginName("XP Pen")]
    public class XP_PenDriverAreaConverter : ConversionFactorAreaConverter
    {
        public const float XP_PEN_AREA_CONSTANT = 3.937f;
        public override DeviceVendor Vendor => DeviceVendor.XP_Pen;

        protected override double GetConversionFactor(TabletState tablet) => XP_PEN_AREA_CONSTANT;
    }
}