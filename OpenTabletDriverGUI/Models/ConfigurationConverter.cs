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
            tablet.TabletName = lines.FindValue<string>("Name");
            if (lines[0].StartsWith("HIDTablet"))
            {
                // Tablet descriptors
                var info = lines.FindValue<string>("HIDTablet").Split(' ');
                tablet.VendorID = int.Parse(info[0], NumberStyles.HexNumber);
                tablet.ProductID = int.Parse(info[1], NumberStyles.HexNumber);
                // Tablet properties
                tablet.Width = lines.FindValue<int>("Width");
                tablet.Height = lines.FindValue<int>("Height");
                tablet.MaxX = lines.FindValue<int>("MaxX");
                tablet.MaxY = lines.FindValue<int>("MaxY");
                tablet.MaxPressure = lines.FindValue<uint>("MaxPressure");
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
                return lines.Convert<T>();
            }
            else
                throw new KeyNotFoundException($"Failed to find key '{key}'.");
        }
    }
}