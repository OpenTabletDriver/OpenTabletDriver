using System;
using System.Collections.Generic;
using System.Linq;
using TabletDriverLib.Class;
using System.Globalization;

namespace OpenTabletDriverGUI.Models
{
    public static class ConfigurationConverter
    {
        public static TabletProperties ConvertHawku(IEnumerable<string> hawkuConfigLines)
        {
            var tablet = new TabletProperties();
            var lines = hawkuConfigLines.ToList();
            tablet.TabletName = lines.FindValue<string>("Name").Replace("\"", "");
            if (lines[0].StartsWith("HIDTablet"))
            {
                // Tablet descriptors
                var info = lines.FindValue<string>("HIDTablet").Split(' ');
                tablet.VendorID = int.Parse(info[0].Replace("0x", ""), NumberStyles.HexNumber);
                tablet.ProductID = int.Parse(info[1].Replace("0x", ""), NumberStyles.HexNumber);
                // Tablet properties
                tablet.Width = lines.FindValue<float>("Width");
                tablet.Height = lines.FindValue<float>("Height");
                tablet.MaxX = lines.FindValue<float>("MaxX");
                tablet.MaxY = lines.FindValue<float>("MaxY");
                tablet.MaxPressure = lines.FindValue<uint>("MaxPressure");
                Log.Info("Converted HIDTablet configuration: " + tablet.TabletName);
            }
            else if (lines[0].StartsWith("USBTablet"))
            {
                // TODO: USBTablet conversion
                throw new NotImplementedException("USBTablet conversion not supported.");
            }
            else
            {
                throw new ArgumentException("Configuration is not a hawku configuration.");
            }
            return tablet;
        }

        private static T FindValue<T>(this IEnumerable<string> lines, string key, string valSplitter = " ")
        {
            var searchResult = lines.FirstOrDefault(line => line.StartsWith(key));
            if (searchResult != null)
            {
                var value = searchResult.Replace(key + valSplitter, string.Empty);
                return value.Convert<T>();
            }
            else
                throw new KeyNotFoundException($"Failed to find key '{key}'.");
        }
    }
}