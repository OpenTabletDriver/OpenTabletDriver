using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using TabletDriverLib;
using TabletDriverPlugin;
using TabletDriverPlugin.Tablet;

namespace OpenTabletDriver.Tools
{
    public static class ConfigurationConverter
    {
        public static IEnumerable<TabletProperties> ConvertHawkuConfigurationFile(IList<string> contents)
        {
            var tablets = new List<TabletProperties>();
            var fileConfigs = contents.Where(l => l.StartsWith("HIDTablet") || l.StartsWith("USBTablet")).ToList();
            var queue = new Queue<int>(fileConfigs.ConvertAll(e => contents.IndexOf(e)));

            while (queue.TryDequeue(out var index))
            {
                if (!queue.TryPeek(out var nextIndex))
                    nextIndex = contents.Count;
                var lines = contents.Skip(index).Take(nextIndex - index).ToList();

                if (lines.Count > 1)
                {
                    var config = ConvertHawku(lines);
                    if (config != null)
                        yield return config;
                }
                else if (lines.Any(line => line.StartsWith("HIDTablet")) || lines.Any(line => line.StartsWith("USBTablet")))
                    continue;
            }
        }

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
                tablet.InputReportLength = lines.FindValue<uint>("ReportLength");
                // Tablet properties
                tablet.Width = lines.FindValue<float>("Width");
                tablet.Height = lines.FindValue<float>("Height");
                tablet.MaxX = lines.FindValue<float>("MaxX");
                tablet.MaxY = lines.FindValue<float>("MaxY");
                tablet.MaxPressure = lines.FindValue<uint>("MaxPressure");
                var mask = lines.FindValue<string>("DetectMask");
                tablet.ActiveReportID = uint.Parse(mask.Replace("0x", ""), NumberStyles.HexNumber);
                Log.Write("Hawku Converter", "Converted HIDTablet configuration: " + tablet.TabletName);
            }
            else if (lines[0].StartsWith("USBTablet"))
            {
                return null;
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

        public static readonly IEnumerable<string> SupportedConfigurations = new List<string>
        {
            "HIDTablet",
            "USBTablet"
        };
    }
}